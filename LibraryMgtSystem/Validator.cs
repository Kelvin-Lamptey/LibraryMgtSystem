using System;

namespace LibraryMgtSystem
{
    public static class Validator
    {
        public static void ValidateBook(string id, string title, string author, string isbn, int totalCopies)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Book ID cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Book title cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(author))
            {
                throw new ArgumentException("Book author cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(isbn))
            {
                throw new ArgumentException("ISBN cannot be empty.");
            }

            // Strip dashes and spaces before checking digits
            string cleanedIsbn = isbn.Replace("-", "").Replace(" ", "");
            if (cleanedIsbn.Length != 10 && cleanedIsbn.Length != 13)
            {
                throw new ArgumentException("ISBN must be 10 or 13 digits.");
            }

            foreach (char c in cleanedIsbn)
            {
                if (!char.IsDigit(c))
                {
                    throw new ArgumentException("ISBN can only contain digits, spaces, or dashes.");
                }
            }

            if (totalCopies < 1)
            {
                throw new ArgumentException("Total copies must be at least 1.");
            }
        }

        public static void ValidateMember(string id, string name, string email)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Member ID cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Member name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be empty.");
            }

            string trimmedEmail = email.Trim();
            if (trimmedEmail.Contains(" "))
            {
                throw new ArgumentException("Email cannot contain spaces.");
            }

            int atIndex = trimmedEmail.IndexOf('@');
            int lastAtIndex = trimmedEmail.LastIndexOf('@');
            if (atIndex <= 0 || atIndex != lastAtIndex)
            {
                throw new ArgumentException("Email must contain a valid '@' symbol.");
            }

            string domainPart = trimmedEmail.Substring(atIndex + 1);
            int dotIndex = domainPart.IndexOf('.');
            if (dotIndex <= 0 || dotIndex >= domainPart.Length - 1)
            {
                throw new ArgumentException("Email domain must contain a '.' with text before and after it.");
            }
        }
    }
}
