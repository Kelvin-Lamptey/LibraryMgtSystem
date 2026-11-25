using System;

namespace LibraryMgtSystem.Models
{
    public class Book
    {
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public string Isbn { get; private set; }
        public int TotalCopies { get; private set; }
        public int AvailableCopies { get; private set; }

        public bool IsAvailable
        {
            get { return AvailableCopies > 0; }
        }

        public Book(string id, string title, string author, string isbn, int totalCopies)
        {
            Id = id;
            Title = title;
            Author = author;
            Isbn = isbn;
            TotalCopies = totalCopies;
            AvailableCopies = totalCopies;
        }

        public void BorrowCopy()
        {
            if (AvailableCopies <= 0)
            {
                throw new InvalidOperationException("No copies available for this book.");
            }

            AvailableCopies--;
        }

        public void ReturnCopy()
        {
            if (AvailableCopies >= TotalCopies)
            {
                throw new InvalidOperationException("All copies of this book are already in the library.");
            }

            AvailableCopies++;
        }

        public override string ToString()
        {
            return $"[{Id}] {Title} by {Author} (ISBN: {Isbn}) - Available: {AvailableCopies}/{TotalCopies}";
        }
    }
}
