using System;
using System.Collections.Generic;
using System.Globalization;
using LibraryMgtSystem.Models;

namespace LibraryMgtSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Library library = new Library();
            library.SeedSampleData();

            bool running = true;
            while (running)
            {
                PrintMenu();
                Console.Write("Select an option: ");
                string choice = Console.ReadLine();

                if (choice == null)
                {
                    break;
                }

                Console.WriteLine();

                try
                {
                    switch (choice.Trim())
                    {
                        case "1":
                            ListBooks(library);
                            break;
                        case "2":
                            AddNewBook(library);
                            break;
                        case "3":
                            SearchBooks(library);
                            break;
                        case "4":
                            ListMembers(library);
                            break;
                        case "5":
                            RegisterMember(library);
                            break;
                        case "6":
                            BorrowBook(library);
                            break;
                        case "7":
                            ReturnBook(library);
                            break;
                        case "8":
                            ListActiveLoans(library);
                            break;
                        case "9":
                            ListOverdueLoans(library);
                            break;
                        case "10":
                            ListAllLoans(library);
                            break;
                        case "0":
                            running = false;
                            Console.WriteLine("Goodbye.");
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please enter a number from the menu.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                if (running)
                {
                    Console.WriteLine();
                }
            }
        }

        static void PrintMenu()
        {
            Console.WriteLine("====================================");
            Console.WriteLine("     Library Management System");
            Console.WriteLine("====================================");
            Console.WriteLine("1.  List all books");
            Console.WriteLine("2.  Add a new book");
            Console.WriteLine("3.  Search books");
            Console.WriteLine("4.  List all members");
            Console.WriteLine("5.  Register a new member");
            Console.WriteLine("6.  Borrow a book");
            Console.WriteLine("7.  Return a book");
            Console.WriteLine("8.  View active loans");
            Console.WriteLine("9.  View overdue loans");
            Console.WriteLine("10. View all loan history");
            Console.WriteLine("0.  Exit");
            Console.WriteLine("------------------------------------");
        }

        static void ListBooks(Library library)
        {
            List<Book> books = library.GetAllBooks();
            if (books.Count == 0)
            {
                Console.WriteLine("No books in the catalog.");
                return;
            }

            Console.WriteLine("Books in Library:");
            foreach (Book book in books)
            {
                Console.WriteLine("  " + book);
            }
        }

        static void AddNewBook(Library library)
        {
            Console.Write("Book ID (e.g. B005): ");
            string id = Console.ReadLine();

            Console.Write("Title: ");
            string title = Console.ReadLine();

            Console.Write("Author: ");
            string author = Console.ReadLine();

            Console.Write("ISBN (10 or 13 digits): ");
            string isbn = Console.ReadLine();

            Console.Write("Number of copies: ");
            string copiesInput = Console.ReadLine();

            int copies;
            if (!int.TryParse(copiesInput, out copies))
            {
                Console.WriteLine("Please enter a valid whole number for copies.");
                return;
            }

            Book added = library.AddBook(id, title, author, isbn, copies);
            Console.WriteLine("Book added: " + added);
        }

        static void SearchBooks(Library library)
        {
            Console.Write("Enter search keyword (title, author, ID or ISBN): ");
            string keyword = Console.ReadLine();

            List<Book> results = library.SearchBooks(keyword);
            if (results.Count == 0)
            {
                Console.WriteLine("No matching books found.");
                return;
            }

            Console.WriteLine($"Found {results.Count} book(s):");
            foreach (Book book in results)
            {
                Console.WriteLine("  " + book);
            }
        }

        static void ListMembers(Library library)
        {
            List<Member> members = library.GetAllMembers();
            if (members.Count == 0)
            {
                Console.WriteLine("No registered members.");
                return;
            }

            Console.WriteLine("Registered Members:");
            foreach (Member member in members)
            {
                Console.WriteLine("  " + member);
            }
        }

        static void RegisterMember(Library library)
        {
            Console.Write("Member ID (e.g. M004): ");
            string id = Console.ReadLine();

            Console.Write("Full name: ");
            string name = Console.ReadLine();

            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.WriteLine("Member type: 1 = Student, 2 = Staff, 3 = Guest");
            Console.Write("Choice: ");
            string typeChoice = Console.ReadLine();

            string cleanId = id != null ? id.Trim() : "";
            string cleanName = name != null ? name.Trim() : "";
            string cleanEmail = email != null ? email.Trim() : "";

            Member member;
            switch (typeChoice != null ? typeChoice.Trim() : "")
            {
                case "1":
                    member = new StudentMember(cleanId, cleanName, cleanEmail);
                    break;
                case "2":
                    member = new StaffMember(cleanId, cleanName, cleanEmail);
                    break;
                case "3":
                    member = new GuestMember(cleanId, cleanName, cleanEmail);
                    break;
                default:
                    Console.WriteLine("Invalid member type selected.");
                    return;
            }

            library.AddMember(member);
            Console.WriteLine("Member registered: " + member);
        }

        static void BorrowBook(Library library)
        {
            Console.Write("Member ID: ");
            string memberId = Console.ReadLine();

            Console.Write("Book ID: ");
            string bookId = Console.ReadLine();

            DateTime borrowDate = ReadOptionalDate("Borrow date (yyyy-MM-dd, press Enter for today): ");
            Loan loan = library.BorrowBook(memberId, bookId, borrowDate);

            Console.WriteLine("Book borrowed successfully.");
            Console.WriteLine("  " + loan);
        }

        static void ReturnBook(Library library)
        {
            Console.Write("Loan ID: ");
            string loanId = Console.ReadLine();

            DateTime returnDate = ReadOptionalDate("Return date (yyyy-MM-dd, press Enter for today): ");
            Loan loan = library.ReturnBook(loanId, returnDate);

            Console.WriteLine("Book returned successfully.");
            if (loan.LateFee > 0m)
            {
                Console.WriteLine($"  Late fee charged: {loan.LateFee:C}");
            }
            else
            {
                Console.WriteLine("  Returned on time. No late fee.");
            }
        }

        static void ListActiveLoans(Library library)
        {
            List<Loan> active = library.GetActiveLoans();
            if (active.Count == 0)
            {
                Console.WriteLine("There are no active loans.");
                return;
            }

            Console.WriteLine("Active Loans:");
            foreach (Loan loan in active)
            {
                Console.WriteLine("  " + loan);
            }
        }

        static void ListOverdueLoans(Library library)
        {
            DateTime checkDate = ReadOptionalDate("Check overdue as of date (yyyy-MM-dd, press Enter for today): ");
            List<Loan> overdue = library.GetOverdueLoans(checkDate);

            if (overdue.Count == 0)
            {
                Console.WriteLine("No overdue loans as of " + checkDate.ToString("yyyy-MM-dd") + ".");
                return;
            }

            Console.WriteLine("Overdue Loans:");
            foreach (Loan loan in overdue)
            {
                int daysLate = loan.GetDaysOverdue(checkDate);
                Console.WriteLine($"  {loan} ({daysLate} day(s) late)");
            }
        }

        static void ListAllLoans(Library library)
        {
            List<Loan> allLoans = library.GetAllLoans();
            if (allLoans.Count == 0)
            {
                Console.WriteLine("No loans recorded yet.");
                return;
            }

            Console.WriteLine("Loan History:");
            foreach (Loan loan in allLoans)
            {
                Console.WriteLine("  " + loan);
            }
        }

        static DateTime ReadOptionalDate(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return DateTime.Today;
            }

            DateTime parsed;
            if (DateTime.TryParseExact(input.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return parsed.Date;
            }

            throw new ArgumentException("Invalid date format. Please use yyyy-MM-dd.");
        }
    }
}
