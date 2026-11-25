# Library Management System

A console-based Library Management Engine built in C# (.NET Framework 4.7.2) using object-oriented programming.

## Features

- Add and view books, search by title, author, ID, or ISBN, and track total vs. available copies.
- Register and view members across different membership types (`Student`, `Staff`, `Guest`).
- Borrow and return books with automatic due date calculation based on member rules.
- Block borrowing when no copies are available, when a member hits their active loan limit, when a member tries to borrow the same book twice, or when a member has overdue books.
- Track overdue loans and calculate late fees upon return.
- Validate inputs (non-empty IDs/names/titles, 10- or 13-digit ISBNs, positive copy counts, basic email format, and valid return dates).

## How to Run

### Running the Console Application
1. Open `LibraryMgtSystem.slnx` in Visual Studio.
2. Make sure `LibraryMgtSystem` is set as the startup project.
3. Save the files and hit **Run** (or Start) in the IDE.

The app loads a few sample books and members on startup so you can try borrowing, returning, and viewing loans right away. When borrowing, returning, or checking overdue loans, you can press **Enter** to use today's date or type a date (`yyyy-MM-dd`) to test overdue dates and late fees.

### Running the Tests
1. In the same solution (`LibraryMgtSystem.slnx`) in Visual Studio, right-click `LibraryMgtSystem.Tests` in Solution Explorer and choose **Set as Startup Project**.
2. Save the files and hit **Run** in the IDE.
3. The console window will run all 16 tests and print `[PASS]` or `[FAIL]` for each test along with the summary.

## Class Design

The solution separates data models, validation, core library rules, and console I/O into separate classes:

- **`Book` (`LibraryMgtSystem/Models/Book.cs`)**: Holds book details (`Id`, `Title`, `Author`, `Isbn`, `TotalCopies`, `AvailableCopies`) and methods `BorrowCopy()` and `ReturnCopy()` so copy counts cannot go below `0` or above `TotalCopies`.
- **`Member` (`LibraryMgtSystem/Models/Member.cs`)**: Abstract base class containing shared member fields (`Id`, `Name`, `Email`) and shared helper methods (`CanBorrow`, `CalculateDueDate`, `CalculateLateFee`). Subclasses override the borrowing rule properties:
  - `StudentMember`: up to 3 books, 14-day loan period, 0.50/day late fee
  - `StaffMember`: up to 6 books, 28-day loan period, 0.25/day late fee
  - `GuestMember`: up to 1 book, 7-day loan period, 1.00/day late fee
- **`Loan` (`LibraryMgtSystem/Models/Loan.cs`)**: Tracks a single borrowing transaction (`LoanId`, `BookId`, `MemberId`, `BorrowDate`, `DueDate`, `ReturnDate`, `LateFee`) and checks whether the loan is overdue and by how many days.
- **`Validator` (`LibraryMgtSystem/Validator.cs`)**: Static helper class that validates book fields (including 10/13-digit ISBNs after stripping dashes and spaces) and member fields (including basic email structure).
- **`Library` (`LibraryMgtSystem/Library.cs`)**: Coordinates books, members, and loans, enforcing duplicate checks, borrowing limits, overdue blocks, returns, late fees, and search queries.
- **`Program` (`LibraryMgtSystem/Program.cs`)**: Console menu loop that reads user input, calls `Library` methods, and catches errors so invalid inputs print a message instead of crashing the app.

### Borrowing Rules Summary

| Member Type | Max Active Books | Loan Period | Late Fee / Day |
|-------------|------------------|-------------|----------------|
| Student     | 3                | 14 days     | 0.50           |
| Staff       | 6                | 28 days     | 0.25           |
| Guest       | 1                | 7 days      | 1.00           |

## Engineering Decisions

- **Inheritance for member rules**: Instead of writing `if/else` blocks for `"Student"`, `"Staff"`, and `"Guest"` everywhere in `Library`, `Member` defines abstract rule properties (`MaxBooksAllowed`, `LoanDays`, `LateFeePerDay`) and shared calculation methods. Adding a new member type only requires adding a new subclass.
- **Separating `Library` from `Program`**: Keeping all business rules inside `Library` and `Models` instead of `Program.cs` makes the core logic easy to test directly in `LibraryMgtSystem.Tests` without needing console input.
- **Separate `Validator` class**: Input validation lives in `Validator` so `Library` stays focused on catalog and loan workflows.
- **Optional date parameters on borrow/return**: `BorrowBook` and `ReturnBook` default to `DateTime.Today` for normal use, while overloads accept a specific `DateTime` so due dates, overdue blocks, and late fees can be tested deterministically.

## Assumptions and Known Limitations

- **In-memory storage**: Data is kept in lists while the program is running and resets when the application closes (no database or file persistence).
- **Sample data on startup**: `Program.cs` calls `SeedSampleData()` automatically; if you add a book or member with the same ID as a sample item (`B001`-`B004` or `M001`-`M003`), validation rejects the duplicate ID.
- **Basic ISBN and email checks**: `Validator` checks that an ISBN has 10 or 13 digits (ignoring spaces and dashes) and that an email has `@` and a domain with `.`, rather than validating ISBN checksums or RFC email edge cases.
- **Single-currency late fees**: Late fees use `decimal` values formatted with the system currency (`:C`) and are calculated on return without a separate payment tracking system.
- **No user authentication**: Anyone running the console menu acts as the librarian and can add books, register members, and borrow or return on any member's behalf.
