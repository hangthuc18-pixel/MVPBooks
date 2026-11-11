using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVPBooks.Data;
using MVPBooks.Models;

namespace MVPBooks.Controllers;

public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Books
    public async Task<IActionResult> Index(
        string? searchString,
        string? category,
        string? stockStatus,
        int page = 1)
    {
        const int pageSize = 5;
        page = Math.Max(page, 1);

        var query = _context.Books.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            searchString = searchString.Trim();
            query = query.Where(b =>
                b.Code.Contains(searchString) ||
                b.Name.Contains(searchString) ||
                b.Author.Contains(searchString));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category == category);
        }

        if (stockStatus == "inStock")
        {
            query = query.Where(b => b.Quantity > 0);
        }
        else if (stockStatus == "outOfStock")
        {
            query = query.Where(b => b.Quantity == 0);
        }
        else
        {
            stockStatus = null;
        }

        ViewBag.Categories = await _context.Books
            .AsNoTracking()
            .Select(b => b.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }
        else if (totalPages == 0)
        {
            page = 1;
        }

        var books = await query
            .OrderBy(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentFilter = searchString;
        ViewBag.CurrentCategory = category;
        ViewBag.CurrentStockStatus = stockStatus;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(books);
    }

    // GET: /Books/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var book = await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        return book is null ? NotFound() : View(book);
    }

    // GET: /Books/Create
    public IActionResult Create() => View();

    // POST: /Books/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        NormalizeBook(book);

        if (ModelState.IsValid && await _context.Books.AnyAsync(b => b.Code == book.Code))
        {
            ModelState.AddModelError(nameof(Book.Code), "Mã sách đã tồn tại.");
        }

        if (!ModelState.IsValid) return View(book);

        _context.Books.Add(book);
        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm sách thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Book.Code), "Không thể lưu sách. Vui lòng kiểm tra mã sách có bị trùng không.");
            return View(book);
        }
    }

    // GET: /Books/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var book = await _context.Books.FindAsync(id);
        return book is null ? NotFound() : View(book);
    }

    // POST: /Books/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book)
    {
        if (id != book.Id) return NotFound();

        NormalizeBook(book);

        if (ModelState.IsValid && await _context.Books.AnyAsync(b => b.Code == book.Code && b.Id != id))
        {
            ModelState.AddModelError(nameof(Book.Code), "Mã sách đã tồn tại.");
        }

        if (!ModelState.IsValid) return View(book);

        var existingBook = await _context.Books.FindAsync(id);
        if (existingBook is null) return NotFound();

        existingBook.Code = book.Code;
        existingBook.Name = book.Name;
        existingBook.Author = book.Author;
        existingBook.Category = book.Category;
        existingBook.Quantity = book.Quantity;

        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật sách thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Book.Code), "Không thể cập nhật. Vui lòng kiểm tra mã sách có bị trùng không.");
            return View(book);
        }
    }

    // GET: /Books/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var book = await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        return book is null ? NotFound() : View(book);
    }

    // POST: /Books/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book is null) return NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Xóa sách thành công!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Books/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.TotalBooks = await _context.Books.CountAsync();
        ViewBag.TotalCopies = await _context.Books.SumAsync(b => (int?)b.Quantity) ?? 0;
        ViewBag.TotalCategories = await _context.Books.Select(b => b.Category).Distinct().CountAsync();
        ViewBag.OutOfStock = await _context.Books.CountAsync(b => b.Quantity == 0);
        ViewBag.LowStock = await _context.Books.CountAsync(b => b.Quantity > 0 && b.Quantity <= 5);
        return View();
    }

    private static void NormalizeBook(Book book)
    {
        book.Code = (book.Code ?? string.Empty).Trim().ToUpperInvariant();
        book.Name = (book.Name ?? string.Empty).Trim();
        book.Author = (book.Author ?? string.Empty).Trim();
        book.Category = (book.Category ?? string.Empty).Trim();
    }
}
