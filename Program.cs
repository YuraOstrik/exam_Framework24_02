using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Net;
using System.Reflection;
using Z.Dapper.Plus;

namespace SQL_exam_Framework_17._02
{
    internal class Program
    {

        static string? connectionString;
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            string path = Directory.GetCurrentDirectory();
            builder.SetBasePath(path);
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            connectionString = config.GetConnectionString("DefaultConnection");


            //var repository = new BookStoreClass(connectionString);
            //var newBook = new Book
            //{
            //    Title = "Новая книга",
            //    AuthorId = 1,
            //    PublisherId = 1,
            //    PageCount = 300,
            //    GenreId = 1,
            //    YearPublished = 2025,
            //    CostPrice = 150.00m,
            //    SalePrice = 200.00m
            //};
            //repository.AddBook(newBook);
            //var books = repository.SearchBooks("Новая книга", "Автор 1", "Жанр 1");
            //foreach (var book in books)
            //{
            //    Console.WriteLine($"Название: {book.Title}, Автор: {book.Author.FullName}, Жанр: {book.Genre.Name}");
            //}

            if (!AuthorizeUser())
            {
                Console.WriteLine("Вы неправильно ввели логин или пароль");
                AuthorizeUser();
            }


            try
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine(" 1: Добавить книгу");
                    Console.WriteLine(" 2: Удаление книгу");
                    Console.WriteLine(" 3: Обновление книгу за параметром");
                    Console.WriteLine(" 4: Продаж книг");
                    Console.WriteLine(" 5: Выписать книг");
                    Console.WriteLine(" 6: Создать скидку на книгу");
                    Console.WriteLine(" 7: Зарезервировать книгу");
                    Console.WriteLine(" 8: Искать книгу по параметру (Жанр, Автор, Издатель)");
                    Console.WriteLine(" 0: Выход");

                    int result = int.Parse(Console.ReadLine());
                    switch (result)
                    {
                        case 1:
                            AddBooks();
                            break;
                        case 2:
                            RemoveBooks();
                            break;
                        case 3:
                            UpdateBooks();
                            break;
                        case 4:
                            SellBooks();
                            break;
                        case 5:
                            WriteOffBooks();
                            break;
                        case 6:
                            AddBookToPromotion();
                            break;
                        case 7:
                            ReserveBookForCustomers();
                            break;
                        case 8:
                            SearchBooks();
                            break;
                        case 0:
                            return;
                        default:
                            Console.WriteLine("Ошибка ввода");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        static bool AuthorizeUser()
        {
            string login = "admin", _log;
            int password = 1111, _pass;

            Console.WriteLine("Login -> ");
            _log = Console.ReadLine();

            do
            {
                Console.WriteLine("Password -> ");
            }
            while (!int.TryParse(Console.ReadLine(), out _pass));
            Console.Clear();
            return login == _log && password == _pass;
        }

        static void AddBooks()
        {
            Console.Clear();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string title;
                int authorId, publisherId, genreId, yearPublished, pageCount;
                decimal costPrice, salePrice;

                Console.WriteLine("Название книги: ");
                string newTitle = Console.ReadLine();
                string? new_title = db.Query<string?>("SELECT Title FROM Books WHERE Title = @Title", new { Title = newTitle }).FirstOrDefault();
                if (new_title != null)
                {
                    title = newTitle;
                    Console.WriteLine("--------------");
                }
                else
                {
                    title = newTitle;
                }

                Console.WriteLine("Введите полное имя автора: ");
                string newFullName = Console.ReadLine();
                int? new_authors = db.Query<int?>("SELECT AuthorID FROM Authors WHERE FullName = @FullName", new { FullName = newFullName }).FirstOrDefault();


                if (new_authors != null && new_title != null)
                {
                    Console.WriteLine("Такая книга уже существует (;");
                    Console.ReadKey();
                    return;
                }
                else if (new_authors != null)
                {
                    authorId = new_authors.Value;
                    Console.WriteLine("--------------");
                }
                else
                {
                    var add_authors = new Author { FullName = newFullName };
                    var sqlQuery = "INSERT INTO Authors (FullName) VALUES(@FullName);" +
                        "SELECT SCOPE_IDENTITY();";

                    authorId = db.ExecuteScalar<int>(sqlQuery, new { FullName = newFullName });

                }


                Console.WriteLine("Введите издателя: ");
                string newPublisher = Console.ReadLine();
                int? new_publishers = db.Query<int?>("SELECT * FROM Publishers WHERE Name = @Name", new { Name = newPublisher }).FirstOrDefault();

                if (new_publishers != null)
                {
                    publisherId = new_publishers.Value;
                    Console.WriteLine("--------------");
                }
                else
                {
                    var add_publishers = new Publisher { Name = newPublisher };
                    var sqlQuery = "INSERT INTO Publishers (Name) VALUES(@Name); SELECT SCOPE_IDENTITY();";
                    publisherId = db.ExecuteScalar<int>(sqlQuery, new { Name = newPublisher });
                }

                Console.WriteLine("Введите кол-во страниц:");
                while (!int.TryParse(Console.ReadLine(), out pageCount) || pageCount <= 0)
                {
                    Console.WriteLine("Некорректный ввод; ");
                }

                Console.WriteLine("Введите жанр: ");
                string newGenre = Console.ReadLine();
                int? new_genres = db.Query<int?>("SELECT * FROM Genres WHERE Name = @Name", new { Name = newGenre }).FirstOrDefault();

                if (new_genres != null)
                {
                    genreId = new_genres.Value;
                    Console.WriteLine("--------------");
                }
                else
                {
                    var add_genres = new Genre { Name = newGenre };
                    var sqlQuery = "INSERT INTO Genres (Name) VALUES(@Name); SELECT SCOPE_IDENTITY();";
                    genreId = db.ExecuteScalar<int>(sqlQuery, new { Name = newGenre });
                }

                Console.Write("Введите год публикации: ");
                while (!int.TryParse(Console.ReadLine(), out yearPublished) || yearPublished < 1000)
                {
                    Console.Write($"Некорректный ввод: ");
                }

                Console.Write("Введите закупочную цену: ");
                while (!decimal.TryParse(Console.ReadLine(), out costPrice) || costPrice < 0)
                {
                    Console.Write("Введите корректную цену: ");
                }

                Console.Write("Введите продажную цену: ");
                while (!decimal.TryParse(Console.ReadLine(), out salePrice) || salePrice < 0 || salePrice < costPrice)
                {
                    Console.Write("Продажная цена должна быть выше закупочной: ");
                }

                var book = new Book()
                {
                    Title = title,
                    AuthorId = authorId,
                    PublisherId = publisherId,
                    PageCount = pageCount,
                    GenreId = genreId,
                    YearPublished = yearPublished,
                    CostPrice = costPrice,
                    SalePrice = salePrice
                };

                string common_sqlQuery = "INSERT INTO Books (Title,AuthorID,PublisherID,PageCount,GenreID,YearPublished,CostPrice,SalePrice)" +
                    "VALUES (@Title,@AuthorID,@PublisherID,@PageCount,@GenreID,@YearPublished,@CostPrice,@SalePrice)";
                int rowsAffected = db.Execute(common_sqlQuery, book);
                if (rowsAffected > 0)
                {
                    Console.WriteLine("\n\t===================================");
                    Console.WriteLine("\t///   КНИГА УСПЕШНО ДОБАВЛЕНА  ///");
                    Console.WriteLine("\t===================================\n");

                    Console.WriteLine($" Название:          {book.Title}");
                    Console.WriteLine($" Автор ID:          {book.AuthorId}");
                    Console.WriteLine($" Издатель ID:       {book.PublisherId}");
                    Console.WriteLine($" Количество страниц:{book.PageCount}");
                    Console.WriteLine($" Жанр ID:           {book.GenreId}");
                    Console.WriteLine($" Год публикации:    {book.YearPublished}");
                    Console.WriteLine($" Закупочная цена:   {book.CostPrice:C}");
                    Console.WriteLine($" Продажная цена:    {book.SalePrice:C}");

                    Console.WriteLine("\n\t===================================\n");
                }
                else
                {
                    Console.WriteLine("книга не была добавлена в базу данных.");
                }

            }
            Console.ReadKey();
        }

        static void RemoveBooks()
        {
            Console.Clear();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                int index;
                var books = db.Query<Book>("SELECT * FROM Books").ToList();
                foreach (var book in books)
                {
                    Console.WriteLine($" {book.BookId} - {book.Title}");
                }
                Console.WriteLine("Введите ID книги: ");
                while (!int.TryParse(Console.ReadLine(), out index) || index > books.Count)
                {
                    Console.WriteLine("Ошибка :(");
                }

                var bookDeleted = books[index - 1];
                var sqlQuery = " Delete FROM Books WHERE BookID = @BookID";
                int rowsAffected = db.Execute(sqlQuery, new { BookId = bookDeleted.BookId });
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"Книга удалена.");
                }
                else
                {
                    Console.WriteLine("Ошибка при удалении книги.");
                }
            }
            Console.ReadKey();
        }

        static void UpdateBooks()
        {
            Console.Clear();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                int index;
                var books = db.Query<Book>("SELECT * FROM Books").ToList();
                Console.WriteLine("ID книги - Название книги - Автор ID - Издатель ID - Жанр ID - Год публикации - Количество страниц - Закупочная цена - Продажная цена\n");
                foreach (var book in books)
                {

                    Console.WriteLine($"{book.BookId} - {book.Title} - {book.AuthorId} - {book.PublisherId} - {book.GenreId} - {book.YearPublished} - {book.PageCount} - {book.CostPrice:C} - {book.SalePrice:C}\n");

                }
                Console.WriteLine(" Выведи номер параметра который нужно поменять: ");
                index = int.Parse(Console.ReadLine());
                switch (index)
                {
                    case 1:
                        Console.WriteLine("Введите новое название книги :");
                        string newTitle = Console.ReadLine();
                        Console.WriteLine("Введите ID книги, которую хотитие изменить: ");
                        int bookIdTitle = int.Parse(Console.ReadLine());
                        db.Execute("UPDATE Books SET Title = @Title WHERE BookID = @BookID", new { BookId = bookIdTitle, Title = newTitle });
                        Console.WriteLine("обновлен!");
                        break;
                    case 2:
                        Console.WriteLine("Введите новый ID автора: ");
                        int newAuthorId = int.Parse(Console.ReadLine());
                        Console.WriteLine("Введите ID книги для обновления: ");
                        int bookIdAuthor = int.Parse(Console.ReadLine());
                        db.Execute("UPDATE Books SET AuthorId = @AuthorId WHERE BookId = @BookId", new { AuthorId = newAuthorId, BookId = bookIdAuthor });
                        Console.WriteLine("обновлен!");
                        break;
                    case 3:
                        Console.WriteLine("Введите новый ID издателя: ");
                        int newPublisherId = int.Parse(Console.ReadLine());
                        Console.WriteLine("Введите ID книги для обновления: ");
                        int bookIdPublisher = int.Parse(Console.ReadLine());
                        db.Execute("UPDATE Books SET PublisherId = @PublisherId WHERE BookId = @BookId", new { PublisherId = newPublisherId, BookId = bookIdPublisher });
                        Console.WriteLine("обновлен!");
                        break;

                    case 4:
                        Console.WriteLine("Введите новый ID жанра: ");
                        int newGenreId = int.Parse(Console.ReadLine());
                        Console.WriteLine("Введите ID книги для обновления: ");
                        int bookIdGenre = int.Parse(Console.ReadLine());
                        db.Execute("UPDATE Books SET GenreId = @GenreId WHERE BookId = @BookId", new { GenreId = newGenreId, BookId = bookIdGenre });
                        Console.WriteLine("обновлен!");
                        break;

                    case 5:
                        Console.WriteLine("Введите новый год публикации: ");
                        int newYearPublished = int.Parse(Console.ReadLine());
                        Console.WriteLine("Введите ID книги для обновления: ");
                        int bookIdYear = int.Parse(Console.ReadLine());
                        db.Execute("UPDATE Books SET YearPublished = @YearPublished WHERE BookId = @BookId", new { YearPublished = newYearPublished, BookId = bookIdYear });
                        Console.WriteLine("обновлен!");
                        break;

                    case 6:
                        Console.WriteLine("Введите новое количество страниц: ");
                        int newPageCount = int.Parse(Console.ReadLine());
                        Console.WriteLine("Введите ID книги для обновления: ");
                        int bookIdPages = int.Parse(Console.ReadLine());
                        db.Execute("UPDATE Books SET PageCount = @PageCount WHERE BookId = @BookId", new { PageCount = newPageCount, BookId = bookIdPages });
                        Console.WriteLine("обновлен!");
                        break;

                    case 7:
                        Console.WriteLine("Введите новую закупочную цену: ");
                        decimal newCostPrice = decimal.Parse(Console.ReadLine());
                        Console.WriteLine("Введите ID книги для обновления: ");
                        int bookIdCost = int.Parse(Console.ReadLine());
                        db.Execute("UPDATE Books SET CostPrice = @CostPrice WHERE BookId = @BookId", new { CostPrice = newCostPrice, BookId = bookIdCost });
                        Console.WriteLine("обновлен!");
                        break;

                    case 8:
                        Console.WriteLine("Введите новую продажную цену: ");
                        decimal newSalePrice = decimal.Parse(Console.ReadLine());
                        Console.WriteLine("Введите ID книги для обновления: ");
                        int bookIdSale = int.Parse(Console.ReadLine());
                        db.Execute("UPDATE Books SET SalePrice = @SalePrice WHERE BookId = @BookId", new { SalePrice = newSalePrice, BookId = bookIdSale });
                        Console.WriteLine("обновлен!");
                        break;

                    case 0:
                        return;
                    default:
                        Console.WriteLine("Ошиька ввода");
                        break;
                }

            }
            Console.ReadKey();
        }

        static void SellBooks()
        {
            Console.Clear();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                int bookId;
                var books = db.Query<Book>("SELECT * FROM Books");
                foreach (var item in books)
                {
                    Console.WriteLine($" {item.BookId} - {item.Title}");
                }
                Console.WriteLine("Введите ID книги которую хотите продать:");
                while (!int.TryParse(Console.ReadLine(), out bookId) || !books.Any(b => b.BookId == bookId))
                {
                    Console.WriteLine("Ошибка: введите корректный ID книги.");
                }


                Console.WriteLine("Введите кол-во книг");
                int quantity = int.Parse(Console.ReadLine());

                var book = db.QueryFirstOrDefault<Book>(
                    "SELECT * FROM Books WHERE BookID = @BookID",
                    new { BookID = bookId });

                if (book == null)
                {
                    Console.WriteLine("Книга не найдена");
                    return;
                }

                var sales = db.Query<Sale>(
                    "SELECT * FROM Sales WHERE BookID = @BookID ORDER BY SaleID",
                    new { book.BookId }).ToList();

                if (!sales.Any())
                {
                    Console.WriteLine("Продаж для данной книги нет");
                    return;
                }

                int sum_quantities = sales.Sum(s => s.Quantity);

                if (quantity > sum_quantities)
                {
                    Console.WriteLine("Ошибка: Вы ввели число больше, чем доступное количество книг.");
                    return;
                }

                foreach (var item in sales)
                {
                    if (quantity <= 0) break;

                    if (item.Quantity > quantity)
                    {
                        db.Execute(
                            "UPDATE Sales SET Quantity = Quantity - @SellQ WHERE SaleID = @SaleID",
                            new { SellQ = quantity, SaleID = item.SaleId });
                        quantity = 0;
                    }
                    else
                    {
                        db.Execute("DELETE FROM Sales WHERE SaleID = @SaleID",
                            new { SaleID = item.SaleId });
                        quantity -= item.Quantity;
                    }
                }

                Console.WriteLine("Продажа успешно завершена");

            }
            Console.ReadKey();
        }

        static void WriteOffBooks()
        {
            Console.Clear();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                int bookId;
                string reason;
                var all_books = db.Query<Book>("SELECT * FROM Books");
                foreach (var item in all_books)
                {
                    Console.WriteLine($" {item.BookId} - {item.Title}");
                }
                Console.WriteLine("Введите ID книги которую хотите выписать:");
                while (!int.TryParse(Console.ReadLine(), out bookId) || !all_books.Any(b => b.BookId == bookId))
                {
                    Console.WriteLine("Ошибка: введите корректный ID книги.");
                }
                var book = db.QueryFirstOrDefault<Book>(
                    "SELECT * FROM Books WHERE BookID = @BookID",
                    new { BookID = bookId });
                if (book == null)
                {
                    Console.WriteLine("Книга не найдена");
                    return;
                }
                bool exist = db.ExecuteScalar<int>("SELECT Count(BookID) FROM BookWriteOffs WHERE BookID = @BookID", new { BookId = book.BookId }) > 0;
                if (exist)
                {
                    Console.WriteLine("Эта книга уже внесена в список:");
                }
                else
                {
                    Console.WriteLine("Введите причину:");
                    reason = Console.ReadLine();

                    var writeoffbook = new BookWriteOff
                    {
                        BookId = book.BookId,
                        WriteOffDate = DateTime.Now,
                        Reason = reason
                    };
                    db.Execute("INSERT INTO BookWriteOffs (BookID, WriteOffDate, Reason)" +
                                      "VALUES (@BookID, @WriteOffDate, @Reason)", writeoffbook);
                    Console.WriteLine("Книгу выписано;");
                }
            }
            Console.ReadKey();
        }

        static void AddBookToPromotion()
        {
            Console.Clear();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                int bookId;
                string name;
                decimal discountPercentage;
                var all_books = db.Query<Book>("SELECT * FROM Books");
                foreach (var item in all_books)
                {
                    Console.WriteLine($" {item.BookId} - {item.Title}");
                }
                Console.WriteLine("Введите ID книги которой вы хотити сделать скидку:");
                while (!int.TryParse(Console.ReadLine(), out bookId) || !all_books.Any(b => b.BookId == bookId))
                {
                    Console.WriteLine("Ошибка: введите корректный ID книги.");
                }
                var book = db.QueryFirstOrDefault<Book>(
                    "SELECT * FROM Books WHERE BookID = @BookID",
                    new { BookID = bookId });

                var bookdisc = db.QueryFirstOrDefault<BookDiscount>(
                    "SELECT * FROM BookDiscounts WHERE BookID = @BookID",
                    new { BookId = book.BookId });

                if (bookdisc != null)
                {
                    Console.WriteLine($"Скидка для книги: {book.BookId}-{book.Title} уже существует.");
                }
                else
                {
                    Console.WriteLine("У этой книги ранее не было скидок, создаем новую...");
                    Console.WriteLine("Введите название скидки:");
                    name = Console.ReadLine();

                    Console.WriteLine("Введите процент скидки (0-100%):");
                    discountPercentage = Convert.ToDecimal(Console.ReadLine());

                    DateTime startDate = DateTime.Now;

                    Console.WriteLine("Введите дату окончания скидки (год-месяц-день):");
                    DateTime endDate;
                    while (!DateTime.TryParse(Console.ReadLine(), out endDate) || endDate <= startDate)
                    {
                        Console.WriteLine("Ошибка: дата окончания должна быть позже даты начала.");
                    }

                    var discount = new Discount
                    {
                        Name = name,
                        DiscountPercentage = discountPercentage,
                        StartDate = startDate,
                        EndDate = endDate
                    };


                    db.Execute(
                        @"INSERT INTO Discounts (Name, DiscountPercentage, StartDate, EndDate) 
                        VALUES (@Name, @DiscountPercentage, @StartDate, @EndDate)",
                        discount);

                    int new_discountId = db.QuerySingle<int>("SELECT CAST(SCOPE_IDENTITY() AS INT)");

                    Console.WriteLine($"Новый DiscountID: {new_discountId}");

                    db.Execute(
                        "INSERT INTO BookDiscounts (BookID, DiscountID) VALUES (@BookID, @DiscountID)",
                        new { BookId = book.BookId, DiscountId = new_discountId });

                    Console.WriteLine("Скидка успешно создана и связана с книгой!");
                }


            }
            Console.ReadKey();
        }

        static void ReserveBookForCustomers()
        {
            Console.Clear();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                int bookId;
                string customerName;
                DateTime reservationDate, expirationDate;

                var all_books = db.Query<Book>("SELECT * FROM Books");
                foreach (var item in all_books)
                {
                    Console.WriteLine($" {item.BookId} - {item.Title}");
                }
                Console.WriteLine("Введите ID книги которую хотите выписать:");
                while (!int.TryParse(Console.ReadLine(), out bookId) || !all_books.Any(b => b.BookId == bookId))
                {
                    Console.WriteLine("Ошибка: введите корректный ID книги.");
                }
                var book = db.QueryFirstOrDefault<Book>(
                    "SELECT * FROM Books WHERE BookID = @BookID",
                    new { BookID = bookId });

                bool reserv = db.ExecuteScalar<int>("SELECT * From Reservations Where BookID = @BookID", new { BookId = book.BookId }) > 0;
                if (!reserv)
                {
                    Console.WriteLine("Введите Полное имя покупателя; ");
                    customerName = Console.ReadLine();
                    Console.WriteLine("Введите дату бронировки (год-мусяц-день)");
                    reservationDate = Convert.ToDateTime(Console.ReadLine());
                    Console.WriteLine("Введите дату конца бронировки (год-мусяц-день)");
                    expirationDate = Convert.ToDateTime(Console.ReadLine());

                    var new_reservation = new Reservation
                    {
                        BookId = book.BookId,
                        CustomerName = customerName,
                        ReservationDate = reservationDate,
                        ExpirationDate = expirationDate
                    };

                    string sqlQuery = "INSERT INTO Reservations (BookID, CustomerName, ReservationDate, ExpirationDate) VALUES(@BookID, @CustomerName, @ReservationDate, @ExpirationDate)";
                    int result = db.Execute(sqlQuery, new_reservation);

                    if (result != null)
                    {
                        Console.WriteLine("Книгу уже зарезервировано");
                    }
                    else
                    {
                        Console.WriteLine("Произошли проблемы во время резервации");
                    }
                }
                else
                {
                    Console.WriteLine("Книгу успешно зарезервировали");
                }

            }
            Console.ReadKey();
        }

        static void SearchBooks()
        {
            Console.Clear();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                int result;
                Console.WriteLine("Поиск книги по: (1) Жанру, (2) Автору, (3) Издателю");

                while (!int.TryParse(Console.ReadLine(), out result) || result < 1 || result > 3)
                {
                    Console.WriteLine("Ошибка;");
                }

                Console.WriteLine("Введите название:");
                string searchValue = Console.ReadLine();

                string query = "";
                if (result == 1)
                {
                    query = @"
                SELECT Books.BookID, Books.Title 
                FROM Books 
                JOIN Genres on Books.GenreID = Genres.GenreID 
                WHERE Genres.Name LIKE @SearchValue";
                }
                else if (result == 2)
                {
                    query = @"
                SELECT Books.BookID, Books.Title 
                FROM Books 
                JOIN Authors on Books.AuthorID = Authors.AuthorID 
                WHERE Authors.Name LIKE @SearchValue";
                }
                else if (result == 3)
                {
                    query = @"
                SELECT Books.BookID, Books.Title 
                FROM Books 
                JOIN Publishers on Books.PublisherID = Publishers.PublisherID 
                WHERE Publishers.Name LIKE @SearchValue";
                }

                var books = db.Query<Book>(query, new { SearchValue = searchValue });

                if (!books.Any())
                {
                    Console.WriteLine("Книги не найдены.");
                }
                else
                {
                    foreach (var book in books)
                    {
                        Console.WriteLine($"{book.BookId} - {book.Title}");
                    }
                }
            }
            Console.ReadKey();
        }


    }
}