public class Book {
    public int Id {get; set;}
    public string Title {get; set;} = "";
    public int Author_id {get; set;}
    public int? Borrower_id {get; set;}
    public int Status {get; set;}
    public int? Due_date {get; set;}
}