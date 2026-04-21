# UI Component Quick Reference Guide

Quick copy-paste examples for using the new UI components.

---

## 🔄 Loading Indicator

### Basic Usage
```razor
@* Add to any view with form submission *@
@{
    ViewData["LoadingMessage"] = "Saving changes...";
}
@await Html.PartialAsync("_LoadingIndicator")

@section Scripts {
<script>
    // Show loading on form submit
    document.querySelector('form').addEventListener('submit', function() {
        document.querySelector('.loading-overlay').style.display = 'flex';
    });
</script>
}
```

### Size Variants
```razor
@* Small loading *@
@{
    ViewData["LoadingMessage"] = "Saving...";
    ViewData["LoadingSize"] = "small";
}
@await Html.PartialAsync("_LoadingIndicator")

@* Large loading *@
@{
    ViewData["LoadingMessage"] = "Importing 1,000 records...";
    ViewData["LoadingSize"] = "large";
}
@await Html.PartialAsync("_LoadingIndicator")
```

### Timed Loading (Demo)
```javascript
function showLoadingFor3Seconds() {
    const overlay = document.querySelector('.loading-overlay');
    overlay.style.display = 'flex';
    
    setTimeout(() => {
        overlay.style.display = 'none';
        showToast('Complete!', 'success');
    }, 3000);
}
```

---

## 📭 Empty State

### Search Results Empty
```razor
@if (Model.Count == 0) {
    <div id="noResultsMessage">
        @{
            ViewData["EmptyIcon"] = "🔍";
            ViewData["EmptyTitle"] = "No results found";
            ViewData["EmptyMessage"] = "Try adjusting your search or filters.";
        }
        @await Html.PartialAsync("_EmptyState")
    </div>
}
```

### Initial Empty State with Action
```razor
@if (Model == null || !Model.Any()) {
    @{
        ViewData["EmptyIcon"] = "👥";
        ViewData["EmptyTitle"] = "No sponsors yet";
        ViewData["EmptyMessage"] = "Get started by adding your first sponsor.";
        ViewData["EmptyAction"] = "+ New Sponsor";
        ViewData["EmptyActionUrl"] = "/Sponsors/Create";
    }
    @await Html.PartialAsync("_EmptyState")
}
```

### Empty State with JavaScript Action
```razor
@{
    ViewData["EmptyIcon"] = "📋";
    ViewData["EmptyTitle"] = "No items";
    ViewData["EmptyMessage"] = "Create your first item to get started.";
    ViewData["EmptyAction"] = "+ Add Item";
    ViewData["EmptyActionUrl"] = "javascript:openCreateModal()";
}
@await Html.PartialAsync("_EmptyState")
```

### Common Icons
- 📋 Items/Documents
- 👥 Users/Sponsors/People
- 🔍 Search results
- 📊 Reports/Stats
- 📧 Messages/Notifications
- 🎓 Students
- 💰 Financial/Payments
- ⚙️ Settings

---

## 🍞 Toast Notifications

### Automatic (TempData)
```csharp
// In Controller
TempData["Success"] = "Sponsor created successfully!";
TempData["Error"] = "Failed to save changes.";
TempData["Warning"] = "This action cannot be undone.";
TempData["Info"] = "Your session will expire in 5 minutes.";

// Toast automatically appears on next page load
```

### Manual JavaScript
```javascript
// In view scripts
showToast('Operation completed!', 'success');
showToast('An error occurred', 'error');
showToast('Warning: Low disk space', 'warning');
showToast('New update available', 'info');

// Custom duration (default 5000ms)
showToast('Quick message', 'info', 2000);
```

### Multiple Toasts
```javascript
function showMultipleNotifications() {
    showToast('Starting process...', 'info');
    setTimeout(() => showToast('Step 1 complete', 'success'), 1000);
    setTimeout(() => showToast('Step 2 complete', 'success'), 2000);
    setTimeout(() => showToast('All done!', 'success'), 3000);
}
```

---

## 🔍 Search Debouncing

### Basic Setup
```html
<input type="text" id="searchInput" placeholder="Search..." />

<script src="~/js/search-debounce.js"></script>
<script>
    document.addEventListener('DOMContentLoaded', function() {
        setupDebouncedSearch('searchInput', filterTable, 300);
    });
    
    function filterTable() {
        const searchTerm = document.getElementById('searchInput').value;
        // Your filtering logic here
    }
</script>
```

### With Loading Indicator
```html
<div style="position: relative;">
    <input type="text" id="searchInput" placeholder="Search..." />
    <span id="searchLoader" class="search-loader" style="display:none; position: absolute; right: 12px; top: 50%; transform: translateY(-50%);"></span>
</div>

<script src="~/js/search-debounce.js"></script>
<script>
    setupDebouncedSearch('searchInput', function() {
        const loader = document.getElementById('searchLoader');
        loader.style.display = 'inline-block';
        
        // Your search logic
        filterTable();
        
        setTimeout(() => {
            loader.style.display = 'none';
        }, 500);
    }, 300);
</script>
```

### Custom Debounce Function
```javascript
// Create custom debounced function
const debouncedSearch = createDebouncedSearch(function() {
    console.log('Searching for:', document.getElementById('searchInput').value);
    performSearch();
}, 400);

// Use it
document.getElementById('searchInput').addEventListener('input', debouncedSearch);
```

---

## ✅ Form Validation Feedback

### Invalid Field
```html
<div class="form-group">
    <label class="field-label">Email Address</label>
    <input type="email" class="is-invalid" value="invalid-email" />
    <div class="field-error">Please enter a valid email address</div>
</div>
```

### Valid Field
```html
<div class="form-group">
    <label class="field-label">Email Address</label>
    <input type="email" class="is-valid" value="user@example.com" />
    <div class="field-success">Email format is correct</div>
</div>
```

### Dynamic Validation (JavaScript)
```javascript
document.getElementById('emailInput').addEventListener('blur', function() {
    const input = this;
    const value = input.value;
    const errorDiv = document.getElementById('emailError');
    
    if (!value.includes('@')) {
        input.classList.add('is-invalid');
        input.classList.remove('is-valid');
        errorDiv.textContent = 'Please enter a valid email';
        errorDiv.style.display = 'block';
    } else {
        input.classList.add('is-valid');
        input.classList.remove('is-invalid');
        errorDiv.style.display = 'none';
    }
});
```

---

## 🔘 Button Loading State

### Basic Usage
```html
<button id="saveBtn" class="btn-primary">Save Changes</button>

<script>
document.getElementById('saveBtn').addEventListener('click', function() {
    const btn = this;
    btn.classList.add('btn-loading');
    btn.disabled = true;
    
    // Simulate async operation
    setTimeout(() => {
        btn.classList.remove('btn-loading');
        btn.disabled = false;
        showToast('Saved successfully!', 'success');
    }, 2000);
});
</script>
```

### Form Submission
```html
<form id="myForm">
    <button type="submit" id="submitBtn" class="btn-primary">Submit</button>
</form>

<script>
document.getElementById('myForm').addEventListener('submit', function(e) {
    e.preventDefault();
    const btn = document.getElementById('submitBtn');
    
    btn.classList.add('btn-loading');
    btn.disabled = true;
    
    // Your AJAX submission here
    fetch('/api/submit', {...})
        .then(response => {
            btn.classList.remove('btn-loading');
            btn.disabled = false;
            showToast('Success!', 'success');
        })
        .catch(error => {
            btn.classList.remove('btn-loading');
            btn.disabled = false;
            showToast('Error: ' + error.message, 'error');
        });
});
</script>
```

---

## 🎨 Common Patterns

### Table with Search + Empty State
```razor
<div class="card">
    <h2 class="table-title">Items (@Model.Count)</h2>
    
    <input type="text" id="searchInput" placeholder="🔍 Search items..." />
    
    @if (Model.Any()) {
        <table id="itemsTable">
            <!-- Table content -->
        </table>
        
        <div id="noResults" style="display:none;">
            @{
                ViewData["EmptyIcon"] = "🔍";
                ViewData["EmptyTitle"] = "No results found";
                ViewData["EmptyMessage"] = "Try a different search term.";
            }
            @await Html.PartialAsync("_EmptyState")
        </div>
    } else {
        @{
            ViewData["EmptyIcon"] = "📋";
            ViewData["EmptyTitle"] = "No items yet";
            ViewData["EmptyMessage"] = "Create your first item.";
            ViewData["EmptyAction"] = "+ New Item";
            ViewData["EmptyActionUrl"] = "/Items/Create";
        }
        @await Html.PartialAsync("_EmptyState")
    }
</div>

@section Scripts {
<script src="~/js/search-debounce.js"></script>
<script>
    setupDebouncedSearch('searchInput', filterTable, 300);
    
    function filterTable() {
        const searchTerm = document.getElementById('searchInput').value.toLowerCase();
        const table = document.getElementById('itemsTable');
        const rows = table.querySelectorAll('tbody tr');
        const noResults = document.getElementById('noResults');
        let visibleCount = 0;
        
        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            if (text.includes(searchTerm)) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });
        
        if (visibleCount === 0) {
            table.style.display = 'none';
            noResults.style.display = 'block';
        } else {
            table.style.display = 'table';
            noResults.style.display = 'none';
        }
    }
</script>
}
```

### CRUD Form with Loading + Toast
```razor
<form asp-action="Create" method="post" id="createForm">
    <!-- Form fields -->
    
    <div class="form-actions">
        <button type="submit" id="submitBtn" class="btn-primary">Create</button>
        <a asp-action="Index" class="btn-secondary">Cancel</a>
    </div>
</form>

@{
    ViewData["LoadingMessage"] = "Creating record...";
}
@await Html.PartialAsync("_LoadingIndicator")

@section Scripts {
<script>
    document.getElementById('createForm').addEventListener('submit', function() {
        const btn = document.getElementById('submitBtn');
        const overlay = document.querySelector('.loading-overlay');
        
        btn.classList.add('btn-loading');
        btn.disabled = true;
        overlay.style.display = 'flex';
        
        // Form will submit, or handle with AJAX
    });
</script>
}
```

```csharp
// In Controller
[HttpPost]
public async Task<IActionResult> Create(CreateViewModel model)
{
    if (ModelState.IsValid)
    {
        await _service.CreateAsync(model);
        TempData["Success"] = "Record created successfully!";
        return RedirectToAction("Index");
    }
    
    TempData["Error"] = "Please correct the errors below.";
    return View(model);
}
```

---

## 📱 Mobile-Specific Tips

### Touch-Friendly Buttons
```css
/* All buttons already have these styles */
.btn {
    min-height: 44px; /* Minimum touch target */
    padding: 12px 24px;
    touch-action: manipulation; /* Prevents double-tap zoom */
}
```

### Mobile Input Sizing
```css
/* All inputs already have these styles */
input, select, textarea {
    min-height: 44px;
    font-size: 14px; /* Prevents zoom on focus */
}
```

### Mobile Toast Positioning
```css
/* Responsive toasts are automatic */
@media (max-width: 767px) {
    .toast-container {
        top: 10px;
        right: 10px;
        left: 10px; /* Full-width */
    }
}
```

---

## ♿ Accessibility Tips

### Focus Management
```javascript
// Example: Modal opening
function openModal() {
    const modal = document.getElementById('myModal');
    modal.style.display = 'flex';
    
    // Focus first interactive element
    const firstButton = modal.querySelector('button, input, a');
    if (firstButton) {
        firstButton.focus();
    }
}
```

### ARIA Labels
```html
<!-- Search input -->
<input type="text" id="search" aria-label="Search sponsors" />

<!-- Icon button -->
<button aria-label="Close dialog">
    <span aria-hidden="true">&times;</span>
</button>

<!-- Status indicator -->
<span class="status-badge" role="status">Active</span>
```

### Keyboard Shortcuts
```javascript
// Example: Escape to close
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        closeModal();
        document.querySelector('.loading-overlay').style.display = 'none';
    }
});
```

---

## 🎯 Complete Example: Search Page

```razor
@model List<Sponsor>
@{
    ViewData["Title"] = "Sponsors";
}

<div class="page-header">
    <h1>All Sponsors</h1>
    <button class="btn-primary" onclick="openCreateModal()">+ New Sponsor</button>
</div>

<div class="card">
    <div style="position: relative; margin-bottom: 16px;">
        <input type="text" id="searchInput" placeholder="🔍 Search sponsors..." 
               aria-label="Search sponsors" />
        <span id="searchLoader" class="search-loader" style="display:none; position: absolute; right: 12px; top: 50%; transform: translateY(-50%);"></span>
    </div>
    
    @if (Model.Any()) {
        <table id="sponsorsTable">
            <thead>
                <tr>
                    <th>Sponsor ID</th>
                    <th>Name</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var sponsor in Model) {
                    <tr data-search="@sponsor.SponsorId @sponsor.SponsorName">
                        <td>@sponsor.SponsorId</td>
                        <td>@sponsor.SponsorName</td>
                        <td>
                            <span class="status-badge status-active">Active</span>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
        
        <div id="noResults" style="display:none;">
            @{
                ViewData["EmptyIcon"] = "🔍";
                ViewData["EmptyTitle"] = "No sponsors found";
                ViewData["EmptyMessage"] = "Try different search terms.";
            }
            @await Html.PartialAsync("_EmptyState")
        </div>
    } else {
        @{
            ViewData["EmptyIcon"] = "👥";
            ViewData["EmptyTitle"] = "No sponsors yet";
            ViewData["EmptyMessage"] = "Get started by adding your first sponsor.";
            ViewData["EmptyAction"] = "+ New Sponsor";
            ViewData["EmptyActionUrl"] = "javascript:openCreateModal()";
        }
        @await Html.PartialAsync("_EmptyState")
    }
</div>

@section Scripts {
<script src="~/js/search-debounce.js"></script>
<script>
    // Debounced search
    setupDebouncedSearch('searchInput', filterTable, 300);
    
    function filterTable() {
        const searchTerm = document.getElementById('searchInput').value.toLowerCase();
        const table = document.getElementById('sponsorsTable');
        const rows = table.querySelectorAll('tbody tr');
        const noResults = document.getElementById('noResults');
        const loader = document.getElementById('searchLoader');
        let visibleCount = 0;
        
        // Show loader
        loader.style.display = 'inline-block';
        
        rows.forEach(row => {
            const searchText = row.getAttribute('data-search').toLowerCase();
            if (searchText.includes(searchTerm)) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });
        
        // Hide loader
        setTimeout(() => loader.style.display = 'none', 300);
        
        // Show/hide empty state
        if (visibleCount === 0) {
            table.style.display = 'none';
            noResults.style.display = 'block';
        } else {
            table.style.display = 'table';
            noResults.style.display = 'none';
        }
    }
</script>
}
```

---

**Need Help?** Check [docs/UI_UX_IMPROVEMENTS.md](docs/UI_UX_IMPROVEMENTS.md) for detailed documentation.
