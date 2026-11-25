using System;
using System.Collections.Generic;
using LibraryMgtSystem.Models;

namespace LibraryMgtSystem
{
    public class Library
    {
        private List<Book> books;
        private List<Member> members;
        private List<Loan> loans;
        private int nextLoanNumber;

        public Library()
        {
            books = new List<Book>();
            members = new List<Member>();
            loans = new List<Loan>();
            nextLoanNumber = 1;
        }

        public Book AddBook(string id, string title, string author, string isbn, int totalCopies)
        {
            Validator.ValidateBook(id, title, author, isbn, totalCopies);

            string cleanId = id.Trim();
            if (FindBookById(cleanId) != null)
            {
                throw new InvalidOperationException($"A book with ID '{cleanId}' already exists.");
            }

            Book book = new Book(cleanId, title.Trim(), author.Trim(), isbn.Trim(), totalCopies);
            books.Add(book);
            return book;
        }

        public Member AddMember(Member member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member", "Member cannot be null.");
            }

            Validator.ValidateMember(member.Id, member.Name, member.Email);

            if (FindMemberById(member.Id.Trim()) != null)
            {
                throw new InvalidOperationException($"A member with ID '{member.Id}' already exists.");
            }

            members.Add(member);
            return member;
        }

        public Book FindBookById(string bookId)
        {
            if (string.IsNullOrWhiteSpace(bookId))
            {
                return null;
            }

            string target = bookId.Trim();
            foreach (Book book in books)
            {
                if (book.Id.Equals(target, StringComparison.OrdinalIgnoreCase))
                {
                    return book;
                }
            }

            return null;
        }

        public Member FindMemberById(string memberId)
        {
            if (string.IsNullOrWhiteSpace(memberId))
            {
                return null;
            }

            string target = memberId.Trim();
            foreach (Member member in members)
            {
                if (member.Id.Equals(target, StringComparison.OrdinalIgnoreCase))
                {
                    return member;
                }
            }

            return null;
        }

        public List<Book> SearchBooks(string keyword)
        {
            List<Book> matches = new List<Book>();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return matches;
            }

            string term = keyword.Trim().ToLower();
            foreach (Book book in books)
            {
                if (book.Title.ToLower().Contains(term) ||
                    book.Author.ToLower().Contains(term) ||
                    book.Id.ToLower().Contains(term) ||
                    book.Isbn.ToLower().Contains(term))
                {
                    matches.Add(book);
                }
            }

            return matches;
        }

        public List<Book> GetAllBooks()
        {
            return new List<Book>(books);
        }

        public List<Member> GetAllMembers()
        {
            return new List<Member>(members);
        }

        public Loan BorrowBook(string memberId, string bookId)
        {
            return BorrowBook(memberId, bookId, DateTime.Today);
        }

        public Loan BorrowBook(string memberId, string bookId, DateTime borrowDate)
        {
            if (string.IsNullOrWhiteSpace(memberId))
            {
                throw new ArgumentException("Member ID cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(bookId))
            {
                throw new ArgumentException("Book ID cannot be empty.");
            }

            Member member = FindMemberById(memberId);
            if (member == null)
            {
                throw new ArgumentException($"Member '{memberId.Trim()}' was not found.");
            }

            Book book = FindBookById(bookId);
            if (book == null)
            {
                throw new ArgumentException($"Book '{bookId.Trim()}' was not found.");
            }

            if (!book.IsAvailable)
            {
                throw new InvalidOperationException($"No copies of '{book.Title}' are currently available.");
            }

            List<Loan> activeMemberLoans = GetActiveLoansForMember(member.Id);

            // Block new loans if the member already has something overdue
            foreach (Loan existingLoan in activeMemberLoans)
            {
                if (existingLoan.IsOverdue(borrowDate))
                {
                    throw new InvalidOperationException("Member has overdue books and must return them before borrowing more.");
                }
            }

            foreach (Loan existingLoan in activeMemberLoans)
            {
                if (existingLoan.BookId.Equals(book.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Member already has an active loan for this book.");
                }
            }

            if (!member.CanBorrow(activeMemberLoans.Count))
            {
                throw new InvalidOperationException($"{member.MemberType} members can only borrow up to {member.MaxBooksAllowed} book(s) at a time.");
            }

            book.BorrowCopy();
            DateTime dueDate = member.CalculateDueDate(borrowDate);
            string loanId = $"L{nextLoanNumber:D3}";
            nextLoanNumber++;

            Loan newLoan = new Loan(loanId, book.Id, member.Id, borrowDate, dueDate);
            loans.Add(newLoan);
            return newLoan;
        }

        public Loan ReturnBook(string loanId)
        {
            return ReturnBook(loanId, DateTime.Today);
        }

        public Loan ReturnBook(string loanId, DateTime returnDate)
        {
            if (string.IsNullOrWhiteSpace(loanId))
            {
                throw new ArgumentException("Loan ID cannot be empty.");
            }

            Loan loan = FindLoanById(loanId);
            if (loan == null)
            {
                throw new ArgumentException($"Loan '{loanId.Trim()}' was not found.");
            }

            if (loan.IsReturned)
            {
                throw new InvalidOperationException("This book has already been returned.");
            }

            if (returnDate.Date < loan.BorrowDate.Date)
            {
                throw new ArgumentException("Return date cannot be earlier than the borrow date.");
            }

            Member member = FindMemberById(loan.MemberId);
            Book book = FindBookById(loan.BookId);

            int daysLate = loan.GetDaysOverdue(returnDate);
            decimal lateFee = member != null ? member.CalculateLateFee(daysLate) : 0m;

            loan.MarkReturned(returnDate, lateFee);
            if (book != null)
            {
                book.ReturnCopy();
            }

            return loan;
        }

        public Loan FindLoanById(string loanId)
        {
            if (string.IsNullOrWhiteSpace(loanId))
            {
                return null;
            }

            string target = loanId.Trim();
            foreach (Loan loan in loans)
            {
                if (loan.LoanId.Equals(target, StringComparison.OrdinalIgnoreCase))
                {
                    return loan;
                }
            }

            return null;
        }

        public List<Loan> GetAllLoans()
        {
            return new List<Loan>(loans);
        }

        public List<Loan> GetActiveLoans()
        {
            List<Loan> active = new List<Loan>();
            foreach (Loan loan in loans)
            {
                if (!loan.IsReturned)
                {
                    active.Add(loan);
                }
            }

            return active;
        }

        public List<Loan> GetActiveLoansForMember(string memberId)
        {
            List<Loan> result = new List<Loan>();
            if (string.IsNullOrWhiteSpace(memberId))
            {
                return result;
            }

            string target = memberId.Trim();
            foreach (Loan loan in loans)
            {
                if (!loan.IsReturned && loan.MemberId.Equals(target, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(loan);
                }
            }

            return result;
        }

        public List<Loan> GetOverdueLoans()
        {
            return GetOverdueLoans(DateTime.Today);
        }

        public List<Loan> GetOverdueLoans(DateTime asOfDate)
        {
            List<Loan> overdue = new List<Loan>();
            foreach (Loan loan in loans)
            {
                if (!loan.IsReturned && loan.IsOverdue(asOfDate))
                {
                    overdue.Add(loan);
                }
            }

            return overdue;
        }

        public void SeedSampleData()
        {
            AddBook("B001", "Clean Code", "Robert C. Martin", "978-0132350884", 3);
            AddBook("B002", "The Pragmatic Programmer", "Andrew Hunt", "978-0135957059", 2);
            AddBook("B003", "C# in Depth", "Jon Skeet", "978-1617294532", 2);
            AddBook("B004", "Design Patterns", "Erich Gamma", "978-0201633610", 1);

            AddMember(new StudentMember("M001", "Ama Mensah", "ama.mensah@uni.edu"));
            AddMember(new StaffMember("M002", "Daniel Osei", "d.osei@uni.edu"));
            AddMember(new GuestMember("M003", "Kojo Boateng", "kojo.boateng@gmail.com"));
        }
    }
}
