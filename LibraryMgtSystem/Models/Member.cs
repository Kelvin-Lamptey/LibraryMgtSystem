using System;

namespace LibraryMgtSystem.Models
{
    // Base class for all library members, subclasses define the borrowing rules
    public abstract class Member
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }

        public abstract string MemberType { get; }
        public abstract int MaxBooksAllowed { get; }
        public abstract int LoanDays { get; }
        public abstract decimal LateFeePerDay { get; }

        protected Member(string id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }

        public bool CanBorrow(int currentActiveLoans)
        {
            return currentActiveLoans < MaxBooksAllowed;
        }

        public DateTime CalculateDueDate(DateTime borrowDate)
        {
            return borrowDate.Date.AddDays(LoanDays);
        }

        public decimal CalculateLateFee(int daysLate)
        {
            if (daysLate <= 0)
            {
                return 0m;
            }

            return daysLate * LateFeePerDay;
        }

        public override string ToString()
        {
            return $"[{Id}] {Name} ({MemberType}) - {Email} | Max: {MaxBooksAllowed} books, {LoanDays} days";
        }
    }
}
