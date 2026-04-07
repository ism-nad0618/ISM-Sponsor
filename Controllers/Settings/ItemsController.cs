using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace ISMSponsor.Controllers.Settings
{
    [Authorize(Roles = "admin")]
    public class ItemsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ItemsController> _logger;
        
        public ItemsController(AppDbContext context, ILogger<ItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search, string? gradeLevel, string? status, string? sortBy, string? sortOrder)
        {
            var query = _context.Items.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => 
                    i.ItemId.Contains(search) ||
                    i.ItemName.Contains(search));
                ViewBag.Search = search;
            }

            // Apply grade level filter
            if (!string.IsNullOrWhiteSpace(gradeLevel))
            {
                query = query.Where(i => i.GradeLevel == gradeLevel);
                ViewBag.GradeLevel = gradeLevel;
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                bool isActive = status == "Active";
                query = query.Where(i => i.IsActive == isActive);
                ViewBag.Status = status;
            }

            // Apply sorting
            sortBy = sortBy ?? "ItemName";
            sortOrder = sortOrder ?? "asc";
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            query = sortBy switch
            {
                "ItemId" => sortOrder == "asc" ? query.OrderBy(i => i.ItemId) : query.OrderByDescending(i => i.ItemId),
                "GradeLevel" => sortOrder == "asc" ? query.OrderBy(i => i.GradeLevel) : query.OrderByDescending(i => i.GradeLevel),
                "Status" => sortOrder == "asc" ? query.OrderBy(i => i.IsActive) : query.OrderByDescending(i => i.IsActive),
                _ => sortOrder == "asc" ? query.OrderBy(i => i.ItemName) : query.OrderByDescending(i => i.ItemName)
            };

            var items = await query.ToListAsync();

            // Get distinct grade levels for filter dropdown
            ViewBag.GradeLevels = await _context.Items.Select(i => i.GradeLevel).Distinct().OrderBy(g => g).ToListAsync();
            ViewBag.Categories = await _context.ItemCategories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Item item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            // Check if item already exists
            var existing = await _context.Items.FindAsync(item.ItemId);
            
            if (existing != null)
            {
                return Json(new { success = false, message = "Item with this ID already exists" });
            }

            try
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Item created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to create item: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditInline([FromBody] Item item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var existing = await _context.Items.FindAsync(item.ItemId);
            if (existing == null)
            {
                return Json(new { success = false, message = "Item not found" });
            }

            existing.ItemName = item.ItemName;
            existing.Description = item.Description;
            existing.GradeLevel = item.GradeLevel;
            existing.Currency = item.Currency;
            existing.Status = item.Status;
            existing.IsActive = item.IsActive;
            existing.CategoryId = item.CategoryId;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Item updated successfully" });
            }
            catch
            {
                return Json(new { success = false, message = "Failed to update item" });
            }
        }

        [HttpPost]
        [Route("Settings/Items/ValidateImport")]
        public async Task<IActionResult> ValidateImport(IFormFile file)
        {
            _logger.LogInformation("ValidateImport called");
            
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded" });
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Only CSV files are supported" });
            }

            try
            {
                var items = new List<Item>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                }))
                {
                    items = csv.GetRecords<Item>().ToList();
                }

                if (items.Count == 0)
                {
                    return Json(new { success = false, message = "No valid records found in CSV" });
                }

                // Get all valid categories for validation
                var validCategories = await _context.ItemCategories
                    .Select(c => new { c.CategoryId, c.CategoryName, c.IsActive })
                    .ToListAsync();

                var validationResults = new List<object>();
                int willAdd = 0, willUpdate = 0, hasErrors = 0;

                foreach (var item in items)
                {
                    var itemErrors = new List<string>();
                    string action = "Skip";
                    
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(item.ItemId))
                    {
                        itemErrors.Add("Missing ItemId");
                    }

                    if (string.IsNullOrWhiteSpace(item.ItemName))
                    {
                        itemErrors.Add("Missing ItemName");
                    }

                    // Validate and resolve CategoryId
                    string? resolvedCategoryId = item.CategoryId;
                    if (!string.IsNullOrEmpty(item.CategoryId))
                    {
                        var category = validCategories.FirstOrDefault(c => c.CategoryId == item.CategoryId && c.IsActive);
                        
                        if (category == null)
                        {
                            category = validCategories.FirstOrDefault(c => 
                                c.CategoryName.Equals(item.CategoryId, StringComparison.OrdinalIgnoreCase) && c.IsActive);
                            
                            if (category != null)
                            {
                                resolvedCategoryId = category.CategoryId;
                            }
                            else
                            {
                                itemErrors.Add($"Invalid CategoryId or CategoryName '{item.CategoryId}'");
                            }
                        }
                    }

                    // Check if item exists
                    var existingItem = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.ItemId == item.ItemId);
                    
                    if (itemErrors.Count == 0)
                    {
                        if (existingItem == null)
                        {
                            action = "Add";
                            willAdd++;
                        }
                        else
                        {
                            action = "Update";
                            willUpdate++;
                        }
                    }
                    else
                    {
                        hasErrors++;
                    }

                    validationResults.Add(new
                    {
                        itemId = item.ItemId,
                        itemName = item.ItemName,
                        description = item.Description,
                        gradeLevel = item.GradeLevel,
                        categoryId = resolvedCategoryId,
                        status = item.Status,
                        action,
                        errors = itemErrors,
                        hasError = itemErrors.Count > 0
                    });
                }

                return Json(new
                {
                    success = true,
                    items = validationResults,
                    summary = new
                    {
                        total = items.Count,
                        willAdd,
                        willUpdate,
                        hasErrors
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation failed");
                return Json(new { success = false, message = $"Validation failed: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("Settings/Items/BulkImport")]
        public async Task<IActionResult> BulkImport(IFormFile file)
        {
            _logger.LogInformation("BulkImport called");
            
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded");
                return Json(new { success = false, message = "No file uploaded" });
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid file type: {FileName}", file.FileName);
                return Json(new { success = false, message = "Only CSV files are supported" });
            }

            _logger.LogInformation("Processing file: {FileName}, Size: {Size} bytes", file.FileName, file.Length);

            try
            {
                var items = new List<Item>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                }))
                {
                    items = csv.GetRecords<Item>().ToList();
                }

                _logger.LogInformation("Parsed {Count} items from CSV", items.Count);

                if (items.Count == 0)
                {
                    return Json(new { success = false, message = "No valid records found in CSV" });
                }

                // Get all valid categories (both ID and Name) for validation and lookup
                var validCategories = await _context.ItemCategories
                    .Select(c => new { c.CategoryId, c.CategoryName, c.IsActive })
                    .ToListAsync();
                
                _logger.LogInformation("Found {Count} categories in database: {Categories}", 
                    validCategories.Count, 
                    string.Join(", ", validCategories.Select(c => $"{c.CategoryName} (ID: {c.CategoryId}, Active: {c.IsActive})")));

                int added = 0, updated = 0, skipped = 0;
                var errors = new List<string>();

                foreach (var item in items)
                {
                    try
                    {
                        // Clear navigation properties to avoid tracking conflicts
                        item.Category = null;

                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(item.ItemId))
                        {
                            errors.Add($"Row skipped: Missing ItemId");
                            skipped++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(item.ItemName))
                        {
                            errors.Add($"Item {item.ItemId}: Missing ItemName");
                            skipped++;
                            continue;
                        }

                        // Validate and resolve CategoryId if provided
                        if (!string.IsNullOrEmpty(item.CategoryId))
                        {
                            // First check if it's a valid CategoryId
                            var category = validCategories.FirstOrDefault(c => c.CategoryId == item.CategoryId && c.IsActive);
                            
                            // If not found, check if it's a CategoryName
                            if (category == null)
                            {
                                category = validCategories.FirstOrDefault(c => 
                                    c.CategoryName.Equals(item.CategoryId, StringComparison.OrdinalIgnoreCase) && c.IsActive);
                                
                                if (category != null)
                                {
                                    // Replace the name with the actual CategoryId
                                    _logger.LogInformation("Resolved category name '{Name}' to ID '{Id}' for item {ItemId}", 
                                        item.CategoryId, category.CategoryId, item.ItemId);
                                    item.CategoryId = category.CategoryId;
                                }
                            }
                            
                            // If still not found, skip this item
                            if (category == null)
                            {
                                errors.Add($"Item {item.ItemId}: Invalid CategoryId or CategoryName '{item.CategoryId}'. Available categories: {string.Join(", ", validCategories.Where(c => c.IsActive).Select(c => c.CategoryName))}");
                                skipped++;
                                continue;
                            }
                        }

                        // Check if item exists
                        var existingItem = await _context.Items.FindAsync(item.ItemId);
                        
                        if (existingItem == null)
                        {
                            // Add new item
                            _logger.LogInformation("Adding new item: {ItemId}", item.ItemId);
                            _context.Items.Add(item);
                            added++;
                        }
                        else
                        {
                            // Update existing item
                            _logger.LogInformation("Updating existing item: {ItemId}", item.ItemId);
                            existingItem.ItemName = item.ItemName;
                            existingItem.Description = item.Description;
                            existingItem.GradeLevel = item.GradeLevel;
                            existingItem.Currency = item.Currency;
                            existingItem.Status = item.Status;
                            existingItem.IsActive = item.IsActive;
                            existingItem.CategoryId = item.CategoryId;
                            updated++;
                        }

                        // Save immediately after each operation
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Saved item: {ItemId}", item.ItemId);
                    }
                    catch (Exception itemEx)
                    {
                        _logger.LogError(itemEx, "Error processing item: {ItemId}", item.ItemId ?? "unknown");
                        errors.Add($"Item {item.ItemId ?? "unknown"}: {itemEx.InnerException?.Message ?? itemEx.Message}");
                        skipped++;
                        
                        // Clear any tracked entities to prevent further issues
                        _context.ChangeTracker.Clear();
                    }
                }

                _logger.LogInformation("Import completed: {Added} added, {Updated} updated, {Skipped} skipped", added, updated, skipped);
                
                // Verify items in database
                var totalItemsInDb = await _context.Items.CountAsync();
                _logger.LogInformation("Total items now in database: {Count}", totalItemsInDb);

                var message = $"Import completed: {added} added, {updated} updated, {skipped} skipped";
                if (errors.Any())
                {
                    message += $". Errors: {string.Join("; ", errors.Take(5))}";
                    if (errors.Count > 5)
                    {
                        message += $" (and {errors.Count - 5} more)";
                    }
                }

                return Json(new { 
                    success = true, 
                    message,
                    added,
                    updated,
                    skipped,
                    errors = errors.Take(10).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import failed with exception");
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = $"Import failed: {innerMessage}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportTemplate()
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write header
                csv.WriteField("ItemId");
                csv.WriteField("ItemName");
                csv.WriteField("Description");
                csv.WriteField("GradeLevel");
                csv.WriteField("Currency");
                csv.WriteField("Status");
                csv.WriteField("CategoryId");
                csv.WriteField("IsActive");
                csv.NextRecord();

                // Write sample row
                csv.WriteField("ITEM001");
                csv.WriteField("Textbook - Math");
                csv.WriteField("Mathematics textbook for grade 9");
                csv.WriteField("9");
                csv.WriteField("USD");
                csv.WriteField("Active");
                csv.WriteField("CAT001");
                csv.WriteField("true");
                csv.NextRecord();
            }

            memoryStream.Position = 0;
            return File(memoryStream, "text/csv", "items_template.csv");
        }
    }
}
