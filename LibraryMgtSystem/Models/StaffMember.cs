namespace LibraryMgtSystem.Models
{
    public class StaffMember : Member
    {
        public StaffMember(string id, string name, string email)
            : base(id, name, email)
        {
        }

        public override string MemberType => "Staff";
        public override int MaxBooksAllowed => 6;
        public override int LoanDays => 28;
        public override decimal LateFeePerDay => 0.25m;
    }
}
