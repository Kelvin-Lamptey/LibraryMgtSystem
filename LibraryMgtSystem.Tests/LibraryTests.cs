using System;
using System.Collections.Generic;
using LibraryMgtSystem.Models;

namespace LibraryMgtSystem.Tests
{
    public static class LibraryTests
    {
        public static int RunAll()
        {
            List<Action> tests = new List<Action>
            {
                TestAddValidBook,
                TestRejectDuplicateBookId,
                TestRejectInvalidBookFields,
                TestAddMemberTypes,
                TestRejectDuplicateMemberAndInvalidEmail,
                TestStudentBorrowingRulesAndLimit,
                TestStaffBorrowingRulesAndLimit,
                TestGuestBorrowingRulesAndLimit,
                TestRejectBorrowWhenNoCopiesAvailable,
                TestRejectBorrowingSameBookTwice,
                TestBlockBorrowWhenMemberHasOverdueBook,
                TestOnTimeReturnRestoresCopyAndZeroFee,
                TestLateReturnCalculatesFeesPerMemberType,
                TestRejectDoubleReturn,
                TestRejectReturnBeforeBorrowDate,
                TestGetOverdueLoansOnlyReturnsUnreturnedOverdue
            };

            int passed = 0;
            int failed = 0;

            foreach (Action test in tests)
            {
                try
                {
                    test();
                    passed++;
                    Console.WriteLine("[PASS] " + test.Method.Name);
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine("[FAIL] " + test.Method.Name + " -> " + ex.Message);
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Total: {tests.Count}, Passed: {passed}, Failed: {failed}");
            return failed;
        }

        static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }

        static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new Exception($"{message} (Expected: {expected}, Actual: {actual})");
            }
        }

        static void AssertThrows<TException>(Action action, string message) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new Exception($"{message} (Expected {typeof(TException).Name}, got {ex.GetType().Name})");
            }

            throw new Exception($"{message} (Expected {typeof(TException).Name}, but no exception was thrown)");
        }

        static void TestAddValidBook()
        {
            Library library = new Library();
            Book book = library.AddBook("B001", "Clean Code", "Robert C. Martin", "978-0132350884", 3);

            AssertEqual("B001", book.Id, "Book ID should match");
            AssertEqual(3, book.TotalCopies, "Total copies should be 3");
            AssertEqual(3, book.AvailableCopies, "Available copies should start at 3");
            AssertTrue(book.IsAvailable, "Book should be available");
        }

        static void TestRejectDuplicateBookId()
        {
            Library library = new Library();
            library.AddBook("B001", "Clean Code", "Robert C. Martin", "978-0132350884", 2);

            AssertThrows<InvalidOperationException>(() =>
            {
                library.AddBook("b001", "Another Book", "Someone", "0132350884", 1);
            }, "Duplicate book ID should throw");
        }

        static void TestRejectInvalidBookFields()
        {
            Library library = new Library();

            AssertThrows<ArgumentException>(() =>
            {
                library.AddBook("B001", "", "Author", "9780132350884", 1);
            }, "Empty title should throw");

            AssertThrows<ArgumentException>(() =>
            {
                library.AddBook("B002", "Title", "Author", "12345", 1);
            }, "Short ISBN should throw");

            AssertThrows<ArgumentException>(() =>
            {
                library.AddBook("B003", "Title", "Author", "9780132350884", 0);
            }, "Zero copies should throw");
        }

        static void TestAddMemberTypes()
        {
            Library library = new Library();
            Member student = library.AddMember(new StudentMember("M001", "Ama", "ama@uni.edu"));
            Member staff = library.AddMember(new StaffMember("M002", "Dan", "dan@uni.edu"));
            Member guest = library.AddMember(new GuestMember("M003", "Kojo", "kojo@mail.com"));

            AssertEqual("Student", student.MemberType, "Student type should match");
            AssertEqual("Staff", staff.MemberType, "Staff type should match");
            AssertEqual("Guest", guest.MemberType, "Guest type should match");
            AssertEqual(3, library.GetAllMembers().Count, "Should have 3 members");
        }

        static void TestRejectDuplicateMemberAndInvalidEmail()
        {
            Library library = new Library();
            library.AddMember(new StudentMember("M001", "Ama", "ama@uni.edu"));

            AssertThrows<InvalidOperationException>(() =>
            {
                library.AddMember(new StaffMember("M001", "Other", "other@uni.edu"));
            }, "Duplicate member ID should throw");

            AssertThrows<ArgumentException>(() =>
            {
                library.AddMember(new GuestMember("M002", "Bad Email", "not-an-email"));
            }, "Invalid email should throw");
        }

        static void TestStudentBorrowingRulesAndLimit()
        {
            Library library = new Library();
            library.AddMember(new StudentMember("M001", "Ama", "ama@uni.edu"));
            library.AddBook("B001", "Book 1", "Author 1", "1111111111", 2);
            library.AddBook("B002", "Book 2", "Author 2", "2222222222", 2);
            library.AddBook("B003", "Book 3", "Author 3", "3333333333", 2);
            library.AddBook("B004", "Book 4", "Author 4", "4444444444", 2);

            DateTime borrowDate = new DateTime(2026, 11, 1);
            Loan firstLoan = library.BorrowBook("M001", "B001", borrowDate);
            library.BorrowBook("M001", "B002", borrowDate);
            library.BorrowBook("M001", "B003", borrowDate);

            AssertEqual(new DateTime(2026, 11, 15), firstLoan.DueDate, "Student due date should be +14 days");

            AssertThrows<InvalidOperationException>(() =>
            {
                library.BorrowBook("M001", "B004", borrowDate);
            }, "Student should not be allowed a 4th active loan");
        }

        static void TestStaffBorrowingRulesAndLimit()
        {
            Library library = new Library();
            library.AddMember(new StaffMember("M002", "Dan", "dan@uni.edu"));

            for (int i = 1; i <= 7; i++)
            {
                library.AddBook($"B00{i}", $"Book {i}", "Author", $"100000000{i}", 1);
            }

            DateTime borrowDate = new DateTime(2026, 11, 1);
            Loan firstLoan = library.BorrowBook("M002", "B001", borrowDate);
            AssertEqual(new DateTime(2026, 11, 29), firstLoan.DueDate, "Staff due date should be +28 days");

            for (int i = 2; i <= 6; i++)
            {
                library.BorrowBook("M002", $"B00{i}", borrowDate);
            }

            AssertEqual(6, library.GetActiveLoansForMember("M002").Count, "Staff should be able to hold 6 active loans");

            AssertThrows<InvalidOperationException>(() =>
            {
                library.BorrowBook("M002", "B007", borrowDate);
            }, "Staff should not exceed 6 active loans");
        }

        static void TestGuestBorrowingRulesAndLimit()
        {
            Library library = new Library();
            library.AddMember(new GuestMember("M003", "Kojo", "kojo@mail.com"));
            library.AddBook("B001", "Book 1", "Author", "1111111111", 1);
            library.AddBook("B002", "Book 2", "Author", "2222222222", 1);

            DateTime borrowDate = new DateTime(2026, 11, 1);
            Loan loan = library.BorrowBook("M003", "B001", borrowDate);

            AssertEqual(new DateTime(2026, 11, 8), loan.DueDate, "Guest due date should be +7 days");

            AssertThrows<InvalidOperationException>(() =>
            {
                library.BorrowBook("M003", "B002", borrowDate);
            }, "Guest should be limited to 1 active loan");
        }

        static void TestRejectBorrowWhenNoCopiesAvailable()
        {
            Library library = new Library();
            library.AddMember(new StudentMember("M001", "Ama", "ama@uni.edu"));
            library.AddMember(new StudentMember("M002", "Yaw", "yaw@uni.edu"));
            library.AddBook("B001", "Single Copy", "Author", "1111111111", 1);

            library.BorrowBook("M001", "B001", new DateTime(2026, 11, 1));

            AssertThrows<InvalidOperationException>(() =>
            {
                library.BorrowBook("M002", "B001", new DateTime(2026, 11, 2));
            }, "Borrowing with 0 copies available should throw");
        }

        static void TestRejectBorrowingSameBookTwice()
        {
            Library library = new Library();
            library.AddMember(new StudentMember("M001", "Ama", "ama@uni.edu"));
            library.AddBook("B001", "Clean Code", "Robert C. Martin", "9780132350884", 3);

            library.BorrowBook("M001", "B001", new DateTime(2026, 11, 1));

            AssertThrows<InvalidOperationException>(() =>
            {
                library.BorrowBook("M001", "B001", new DateTime(2026, 11, 2));
            }, "Same member borrowing same book twice concurrently should throw");
        }

        static void TestBlockBorrowWhenMemberHasOverdueBook()
        {
            Library library = new Library();
            library.AddMember(new StudentMember("M001", "Ama", "ama@uni.edu"));
            library.AddBook("B001", "Book 1", "Author", "1111111111", 2);
            library.AddBook("B002", "Book 2", "Author", "2222222222", 2);

            // Borrow on Nov 1 (due Nov 15)
            library.BorrowBook("M001", "B001", new DateTime(2026, 11, 1));

            // Try to borrow another book on Nov 16 while B001 is overdue
            AssertThrows<InvalidOperationException>(() =>
            {
                library.BorrowBook("M001", "B002", new DateTime(2026, 11, 16));
            }, "Member with overdue book should be blocked from borrowing");
        }

        static void TestOnTimeReturnRestoresCopyAndZeroFee()
        {
            Library library = new Library();
            library.AddMember(new StudentMember("M001", "Ama", "ama@uni.edu"));
            Book book = library.AddBook("B001", "Book 1", "Author", "1111111111", 1);

            Loan loan = library.BorrowBook("M001", "B001", new DateTime(2026, 11, 1));
            AssertEqual(0, book.AvailableCopies, "Available copies should drop to 0");

            Loan returned = library.ReturnBook(loan.LoanId, new DateTime(2026, 11, 15));
            AssertTrue(returned.IsReturned, "Loan should be marked returned");
            AssertEqual(0m, returned.LateFee, "On-time return should have 0 late fee");
            AssertEqual(1, book.AvailableCopies, "Available copies should be restored to 1");
        }

        static void TestLateReturnCalculatesFeesPerMemberType()
        {
            Library library = new Library();
            library.AddMember(new StudentMember("M001", "Ama", "ama@uni.edu"));
            library.AddMember(new StaffMember("M002", "Dan", "dan@uni.edu"));
            library.AddMember(new GuestMember("M003", "Kojo", "kojo@mail.com"));

            library.AddBook("B001", "Book 1", "Author", "1111111111", 3);

            DateTime borrowDate = new DateTime(2026, 11, 1);
            Loan studentLoan = library.BorrowBook("M001", "B001", borrowDate); // due Nov 15
            Loan staffLoan = library.BorrowBook("M002", "B001", borrowDate);   // due Nov 29
            Loan guestLoan = library.BorrowBook("M003", "B001", borrowDate);   // due Nov 8

            // Return each 4 days late
            library.ReturnBook(studentLoan.LoanId, new DateTime(2026, 11, 19));
            library.ReturnBook(staffLoan.LoanId, new DateTime(2026, 12, 3));
            library.ReturnBook(guestLoan.LoanId, new DateTime(2026, 11, 12));

            AssertEqual(2.00m, studentLoan.LateFee, "Student 4 days late fee should be 2.00");
            AssertEqual(1.00m, staffLoan.LateFee, "Staff 4 days late fee should be 1.00");
            AssertEqual(4.00m, guestLoan.LateFee, "Guest 4 days late fee should be 4.00");
        }

        static void TestRejectDoubleReturn()
        {
            Library library = new Library();
            library.AddMember(new StudentMember("M001", "Ama", "ama@uni.edu"));
            library.AddBook("B001", "Book 1", "Author", "1111111111", 1);

            Loan loan = library.BorrowBook("M001", "B001", new DateTime(2026, 11, 1));
            library.ReturnBook(loan.LoanId, new DateTime(2026, 11, 5));

            AssertThrows<InvalidOperationException>(() =>
            {
                library.ReturnBook(loan.LoanId, new DateTime(2026, 11, 6));
            }, "Double return should throw");
        }

        static void TestRejectReturnBeforeBorrowDate()
        {
            Library library = new Library();
            library.AddMember(new StudentMember("M001", "Ama", "ama@uni.edu"));
            library.AddBook("B001", "Book 1", "Author", "1111111111", 1);

            Loan loan = library.BorrowBook("M001", "B001", new DateTime(2026, 11, 10));

            AssertThrows<ArgumentException>(() =>
            {
                library.ReturnBook(loan.LoanId, new DateTime(2026, 11, 8));
            }, "Return date earlier than borrow date should throw");
        }

        static void TestGetOverdueLoansOnlyReturnsUnreturnedOverdue()
        {
            Library library = new Library();
            library.AddMember(new GuestMember("M001", "Guest 1", "g1@mail.com"));
            library.AddMember(new GuestMember("M002", "Guest 2", "g2@mail.com"));
            library.AddMember(new StaffMember("M003", "Staff 1", "s1@uni.edu"));
            library.AddBook("B001", "Book 1", "Author", "1111111111", 3);

            DateTime borrowDate = new DateTime(2026, 11, 1);
            Loan overdueActive = library.BorrowBook("M001", "B001", borrowDate);   // due Nov 8
            Loan overdueReturned = library.BorrowBook("M002", "B001", borrowDate); // due Nov 8
            library.BorrowBook("M003", "B001", borrowDate);                        // due Nov 29

            // Return M002 late on Nov 10, so it is no longer an active overdue loan
            library.ReturnBook(overdueReturned.LoanId, new DateTime(2026, 11, 10));

            List<Loan> overdue = library.GetOverdueLoans(new DateTime(2026, 11, 12));
            AssertEqual(1, overdue.Count, "Only 1 unreturned loan should be overdue");
            AssertEqual(overdueActive.LoanId, overdue[0].LoanId, "Returned loan should not be in overdue list");
        }
    }
}
