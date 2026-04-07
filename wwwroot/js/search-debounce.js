/**
 * Search debouncing utility
 * Prevents excessive filtering on every keystroke
 */

function createDebouncedSearch(callback, delay = 300) {
    let timer;
    return function(...args) {
        clearTimeout(timer);
        timer = setTimeout(() => callback.apply(this, args), delay);
    };
}

/**
 * Setup debounced search on an input element
 * @param {string} inputId - ID of the search input
 * @param {Function} filterFunction - Function to call with search value
 * @param {number} delay - Debounce delay in milliseconds
 */
function setupDebouncedSearch(inputId, filterFunction, delay = 300) {
    const input = document.getElementById(inputId);
    if (!input) {
        console.warn(`Search input with ID "${inputId}" not found`);
        return;
    }

    const debouncedFilter = createDebouncedSearch(filterFunction, delay);
    
    input.addEventListener('input', function(e) {
        debouncedFilter(e.target.value);
    });
    
    // Also trigger on paste
    input.addEventListener('paste', function(e) {
        setTimeout(() => debouncedFilter(e.target.value), 0);
    });
}

/**
 * Show/hide loading indicator during search
 */
function showSearchLoading(show = true) {
    const loader = document.getElementById('searchLoader');
    if (loader) {
        loader.style.display = show ? 'inline-block' : 'none';
    }
}
