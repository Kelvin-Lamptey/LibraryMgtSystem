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
    }
}
