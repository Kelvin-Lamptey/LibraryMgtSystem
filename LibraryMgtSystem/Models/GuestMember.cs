namespace LibraryMgtSystem.Models
{
    public class GuestMember : Member
    {
        public GuestMember(string id, string name, string email)
            : base(id, name, email)
        {
        }

        public override string MemberType => "Guest";
        public override int MaxBooksAllowed => 1;
        public override int LoanDays => 7;
        public override decimal LateFeePerDay => 1.00m;
    }
}
