using System;

class Program {
    public static void Main() {
        Library lib = new Library();
        
        // keep program alive until exit is called
        while (true) {
            Console.WriteLine("\n------Library CLI------");
            Console.WriteLine("Please make a selection from the following: ");
            Console.WriteLine("1) List all books in the library");
            Console.WriteLine("2) Search for a book from a specific author");
            Console.WriteLine("3) Check a book out");
            Console.WriteLine("4) List the overdue books");
            Console.WriteLine("5) Exit the application");
            Console.Write("\nMake a selection: ");

            var selection = Console.ReadLine();
            
            switch (selection) {
                case "1":
                    Console.WriteLine();
                    lib.ListLibrary();
                    break;
                case "2":
                    Console.WriteLine();
                    lib.SearchBook();
                    break;
                case "3":
                    Console.WriteLine();
                    lib.CheckoutBook();
                    break;
                case "4":
                    Console.WriteLine();
                    lib.GetOverdueBooks();
                    break;

                // returns out of the function
                case "5":
                    return;
                case "exit":
                    return;
                default: 
                    Console.WriteLine(selection + " is not valid input. Please try again");
                    break;
            }
        }
    }
}