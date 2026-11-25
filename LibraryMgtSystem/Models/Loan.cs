using System;

namespace LibraryMgtSystem.Models
{
    public class Loan
    {
        public string LoanId { get; private set; }
        public string BookId { get; private set; }
        public string MemberId { get; private set; }
        public DateTime BorrowDate { get; private set; }
        public DateTime DueDate { get; private set; }
        public DateTime? ReturnDate { get; private set; }
        public decimal LateFee { get; private set; }

        public bool IsReturned
        {
            get { return ReturnDate.HasValue; }
        }

        public Loan(string loanId, string bookId, string memberId, DateTime borrowDate, DateTime dueDate)
        {
            LoanId = loanId;
            BookId = bookId;
            MemberId = memberId;
            BorrowDate = borrowDate.Date;
            DueDate = dueDate.Date;
            ReturnDate = null;
            LateFee = 0m;
        }

        public bool IsOverdue(DateTime asOfDate)
        {
            if (IsReturned)
            {
                return ReturnDate.Value.Date > DueDate.Date;
            }

            return asOfDate.Date > DueDate.Date;
        }

        public int GetDaysOverdue(DateTime asOfDate)
        {
            DateTime checkDate = IsReturned ? ReturnDate.Value.Date : asOfDate.Date;
            int days = (checkDate - DueDate.Date).Days;
            return days > 0 ? days : 0;
        }

        public void MarkReturned(DateTime returnDate, decimal lateFee)
        {
            if (IsReturned)
            {
                throw new InvalidOperationException("This loan has already been returned.");
            }

            if (returnDate.Date < BorrowDate.Date)
            {
                throw new ArgumentException("Return date cannot be earlier than borrow date.");
            }

            ReturnDate = returnDate.Date;
            LateFee = lateFee;
        }

        public override string ToString()
        {
            string status;
            if (IsReturned)
            {
                status = $"Returned on {ReturnDate.Value:yyyy-MM-dd}";
                if (LateFee > 0m)
                {
                    status += $" (Late fee: {LateFee:C})";
                }
            }
            else
            {
                status = "Active";
            }

            return $"[{LoanId}] Book: {BookId} | Member: {MemberId} | Borrowed: {BorrowDate:yyyy-MM-dd} | Due: {DueDate:yyyy-MM-dd} | {status}";
        }
    }
}
