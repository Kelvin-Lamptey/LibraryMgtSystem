namespace LibraryMgtSystem.Models
{
    public class StudentMember : Member
    {
        public StudentMember(string id, string name, string email)
            : base(id, name, email)
        {
        }

        public override string MemberType => "Student";
        public override int MaxBooksAllowed => 3;
        public override int LoanDays => 14;
        public override decimal LateFeePerDay => 0.50m;
    }
}
