using Microsoft.Data.Sqlite;

class Library {
    private SqliteConnection connection;
    public Library () {
        // checks to see if file exists if not creates database and adds default data
        if (!File.Exists("./Library.db")) {
            connection = new SqliteConnection("Data Source=./Library.db");
            InitDatabase();
        } else {
            connection = new SqliteConnection("Data Source=./Library.db");
        }
    }

    // lists all of the books in the library 
    public void ListLibrary() {
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        SELECT title, status, id FROM Books";
        var read = cmd.ExecuteReader();

        while(read.Read()) {
            string avail = (read.GetInt32(1) == 1) ? "Available" : "Checked Out";
            Console.WriteLine("Title: " + read[0] + " -- Availibility Status: " + avail + " ID: " + read[2]);
        }

        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }

    // takes in a name and checks the database for authors with that name and displays their books that are availible
    public void SearchBook() {
        connection.Open();
         // stay in the loop until user exits
        while (true) {
            Console.WriteLine("Enter the authors name or type exit to back to main screen");
            var author = Console.ReadLine();
            if (author == null || author.Equals("exit")) { 
                break; 
            }

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            SELECT Books.title, Authors.name
            FROM Books
            JOIN Authors ON Authors.id = Books.author_id
            WHERE Books.status = 1 AND Authors.name LIKE @author";

            cmd.Parameters.AddWithValue("@author", author);
            
            // checks to ensure if there is a book with that id and is avalible was found
            var read = cmd.ExecuteReader();
            while (read.Read()) {
                Console.WriteLine(read[0] + " By: " + read[1] + " is avalible for checkout");
            }

            // hold the user before sending them back to the begining of the loop so they can control app flow
            Console.WriteLine("\nPlease press a key to continue");
            Console.ReadKey();

        }
        connection.Close();
    }

    // asks users for book id and borrow id and attempts to checkout the book 
    // uses ID due to the fact its gurranteed to be unique while title is not
    public void CheckoutBook() {
        connection.Open();

        // stay in the loop until user exits
        while (true) {
            Console.WriteLine("Enter ID of the book or type exit to back back to main screen");
            var book = Console.ReadLine();
            if (book == null || book.Equals("exit")) { 
                break;
            }

            Console.WriteLine("Enter ID of the borrower");
            var borrower = Console.ReadLine();
            if (borrower == null || borrower.Equals("exit")) { 
                break;
            }
            
            // some level of assumption of good input in terms of no characters are given
            int borrowerID = int.Parse(borrower);
            int bookID = int.Parse(book);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            UPDATE Books
            SET borrower_id = @borrower_id, due_date = @due_date, status = 0
            WHERE status = 1 AND id = @id";

            cmd.Parameters.AddWithValue("@borrower_id", borrowerID);
            cmd.Parameters.AddWithValue("@id", bookID);
            cmd.Parameters.AddWithValue("@due_date", DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds());
            
            // checks to ensure if there is a book with that id and is avalible was found
            if (cmd.ExecuteNonQuery() >= 1) {
                Console.WriteLine(bookID + " Has been succesfully checkedout");
            } else {
                Console.WriteLine("Book with id: " + bookID + " either is unavalible or does not exist");
            }
        }

        connection.Close();
    }

    // prints all books that are checked out and have a past due book
    public void GetOverdueBooks() {
        connection.Open();
        using var cmd = connection.CreateCommand();

        cmd.CommandText = @"
        SELECT Books.Title, Borrowers.name, Borrowers.email
        FROM Books
        JOIN Borrowers ON Books.borrower_id = Borrowers.id
        WHERE Books.due_date < @now AND status = 0";

        cmd.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var read = cmd.ExecuteReader();
        while (read.Read()) {
            Console.WriteLine("Title: " + read[0] + " is overdue and checked out by " + read[1] + " : " + read[2]);
        }

        connection.Close();

        // hold the user before sending them back to the main screen so they can control flow
        Console.WriteLine("\nPlease press a key to continue");
        Console.ReadKey();
        
    }

    public void AddAuthor(Author author) {
        using var cmd = connection.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO Authors
            (name) VALUES (@name)";

        cmd.Parameters.AddWithValue("@name", author.Name);
        cmd.ExecuteNonQuery();
    }

    public void AddBook(Book book) {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Books
            (title, author_id, borrower_id, status, due_date) 
            VALUES (@title, @author_id, @borrower_id, @status, @due_date)";

        cmd.Parameters.AddWithValue("@title", book.Title);
        cmd.Parameters.AddWithValue("@author_id", book.Author_id);
        cmd.Parameters.AddWithValue("@status", book.Status);

        // need to ensure that we handle potential null values to add to the database
        if (book.Borrower_id != null && book.Due_date != null) {
            cmd.Parameters.AddWithValue("@borrower_id", book.Borrower_id);
            cmd.Parameters.AddWithValue("@due_date", book.Due_date);
        } else {
            cmd.Parameters.AddWithValue("@borrower_id", DBNull.Value);
            cmd.Parameters.AddWithValue("@due_date", DBNull.Value);
        }

        cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();
    }

    public void AddBorrower(Borrower borrower) {
        using var cmd = connection.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO Borrowers 
            (name, email) VALUES (@name, @email)";

        cmd.Parameters.AddWithValue("@name", borrower.Name);
        cmd.Parameters.AddWithValue("@email", borrower.Email);
        cmd.ExecuteNonQuery();
    }

    // initilizes and loads up the database with fixed data
    private void InitDatabase() {
        connection.Open();
        using var cmd = connection.CreateCommand();

        // creates all the tables Authors, Borrowers, Books
        cmd.CommandText = @"
            CREATE TABLE Authors (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();
        
        cmd.CommandText = @"
            CREATE TABLE Borrowers (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                email TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE Books (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                author_id INTEGER NOT NULL,
                borrower_id INTEGER,
                status INTEGER NOT NULL,
                due_date INTEGER
            );";
        cmd.ExecuteNonQuery();

        // below here fills data to the database
        Author[] authors = [
            new Author { Name = "Andrzej Sapkowski"},
            new Author { Name = "John Tokien"},
            new Author { Name = "Dr. Seuss"},
            new Author { Name = "R.L. Stine"},
            new Author { Name = "William Shakespeare"},
            new Author { Name = "Elie Wiesel"},
            new Author { Name = "Tom Clancy"},
        ];
        
        foreach (Author author in authors) {
            AddAuthor(author);
        }

        Borrower[] borrowers = [
            new Borrower { Name = "Maxwell Calhoun", Email = "Maxwell.Lee.Calhoun@gmail.com"},
            new Borrower { Name = "Jane Doe", Email = "Jane.Doe@gmail.com"},
            new Borrower { Name = "Ada Lovelace", Email = "Ada.Lovalace@gmail.com"},
            new Borrower { Name = "Alan Turing", Email = "TuringMachine@Turing.com"},
            new Borrower { Name = "Blaise Pascal", Email = "Pascal@triangle.com"},
        ];
        
        foreach (Borrower borrower in borrowers) {
            AddBorrower(borrower);
        }

        Book[] books =  [
            new Book { Title = "The Last Wish", Author_id = 1, Borrower_id = 1, Status = 0, Due_date = 1745227400},
            new Book { Title = "The Tower of the Swallow", Author_id = 1, Status = 1 },
            new Book { Title = "The Hobbit", Author_id = 2, Borrower_id = 4, Status = 0, Due_date = 1746005000},
            new Book { Title = "The Fellowship of the Ring", Author_id = 2, Status = 1 },
            new Book { Title = "Green Eggs and Ham", Author_id = 3, Borrower_id = 2, Status = 1, Due_date = 1745918600 },
            new Book { Title = "The Cat in the Hat", Author_id = 3, Borrower_id = 2, Status = 0, Due_date = 1745918600 },
            new Book { Title = "Goosebumps: Night of the Living Dummy", Author_id = 4, Status = 1 },
            new Book { Title = "Goosebumps: Welcome to Dead House", Author_id = 4, Status = 1 },
            new Book { Title = "Hamlet", Author_id = 5, Status = 1 },
            new Book { Title = "Romeo & Juliet", Author_id = 5, Status = 1 },
            new Book { Title = "Night", Author_id = 6, Borrower_id = 1, Status = 0, Due_date = 1745807889 },
        ];
        
        foreach (Book book in books) {
            AddBook(book);
        }

        connection.Close();
    }
}