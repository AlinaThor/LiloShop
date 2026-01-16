using Dapper;
using LiloShop.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;

namespace LiloShop
{
    internal class Program
    {
        private static Database _database = new Database();
        private static Customer _loggedInCustomer = null;
        private const string AdminPassword = "admin123";

        static void Main(string[] args)
        {
            RenderStartpage();
        }

        private static void RenderMainMenu()
        {
            Console.Clear();
            var menuOptions = new List<string>
            {
                "1.Startsidan",
                "2.Se alla artiklar",
                "3.Se alla kategorier",
                "4.Kundkorg",
                "5.Sök",
                "6.Logga in",
                "7.Mina ordrar",
                "8.Logga ut",
                "",
                "9.Admin",
            };
            var mainMenu = new MenuPlaceholder("Huvudmeny", 2, 2, menuOptions);
            mainMenu.Draw();
            var option = Console.ReadLine();

            if (int.TryParse(option, out int value))
            {
                switch (value)
                {
                    case 1:
                        RenderStartpage();
                        break;
                    case 2:
                        RenderAllProducts();
                        break;
                    case 3:
                        RenderCategories();
                        break;
                    case 4:
                       RenderBasket();
                        break;
                    case 5:
                        RenderSearch();
                        break;
                    case 6:
                        RenderCustomerLogIn();
                        break;
                    case 7:
                        RenderMyOrders();
                        break;
                    case 8:
                        _loggedInCustomer = null;
                        Console.WriteLine("Du har loggat ut");
                        Console.ReadLine();
                        RenderMainMenu();
                        break;

                    case 9:
                        RenderAdminLogIn();
                        break;
                    default:
                        ShowErrorInput(RenderMainMenu);
                        break;
                }
            }
            else
            {
                ShowErrorInput(RenderMainMenu);
            }
        }

        private static void RenderStartpage()
        {
            Console.Clear();

            var welcomeBox = new MenuPlaceholder($"Välkommen till Lilo", 22, 1, new List<string> { "Världens bästa shop i ett console fönster"});
            welcomeBox.Draw();

            var products = _database.Products
                .Include(p => p.Category)
                .Where(x => x.IsSpecialOffer)
                .Take(3)
                .ToList();

            for(var i = 0; i < products.Count; i++)
            {
                var left = 2;
                var product = products[i];


                var offerInfo = new List<string>();
                offerInfo.Add(product.Name);
                offerInfo.Add("Kategori: " + product.Category?.Name); 
                offerInfo.Add(product.Color.ToString());
                offerInfo.Add(product.Price.ToString("C2"));
                offerInfo.Add("");
                switch (i)
                {
                    case 0:
                        left = 2;
                        offerInfo.Add("Klicka A för att köpa");
                        break;
                    case 1:
                        left = 30;
                        offerInfo.Add("Klicka B för att köpa");
                        break;
                    case 2:
                        left = 60;
                        offerInfo.Add("Klicka C för att köpa");
                        break;

                }
             
              
                var offerBox = new MenuPlaceholder($"Erbjudande {i+1}", left, 6, offerInfo);
                offerBox.Draw();
            }

            Console.WriteLine("\n\nVälj erbjudande eller klicka enter för att komma till huvudmenyn");

            var userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput))
            {
                RenderMainMenu();
            }
            else
            {
                switch (userInput.ToLower())
                {
                    case "a":
                        var product = products[0];
                        RenderProduct(product, RenderStartpage);
                        break;
                    case "b":
                        var secondProduct = products[1];
                        RenderProduct(secondProduct, RenderStartpage);
                        break;
                    case "c":
                        var thirdProduct = products[2];
                        RenderProduct(thirdProduct, RenderStartpage);
                        break;
                    default:
                        ShowErrorInput(RenderStartpage, "Du har angivet ett felaktigt erbjudande, försök igen");
                        break;
                }
               
            }
            
        }

        private static void RenderAllProducts()
        {
            Console.Clear();

            var products = _database.Products
                .Include(p => p.Category)
                .ToList();
            RenderProducts(products, RenderMainMenu);

        }

        private static void RenderProducts(List<Product> products, Action goBackAction)
        {
            Console.Clear ();
            var boxOptions = new List<string> { "", "0: Gå tillbaka", };

            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                boxOptions.Add($"{i + 1}. {product.Name} - {product.Price:C2}");
            }

            var box = new MenuPlaceholder($"Shoppen", 0, 0, boxOptions);
            box.Draw();
            Console.Write("Välj produktnummer för mer info: ");


            var input = Console.ReadLine();

            if (int.TryParse(input, out var choice))
            {
                if (choice == 0)
                {
                    goBackAction();
                    return;
                }
                if (choice > 0 && choice <= products.Count)
                {
                    var selectedProduct = products[choice - 1];
                    RenderProduct(selectedProduct, goBackAction);
                }
                else
                {
                    ShowErrorInput(goBackAction);
                    return;
                }

            }
            else
            {
                ShowErrorInput(goBackAction);
                return;
            }
        
        }

        private static void RenderCategories()
        {
            Console.Clear();
            var boxOptions = new List<string>();
            boxOptions.Add("0: Gå tillbaka");
            var categories = _database.Categories.Include(x => x.Products).ToList();
            foreach(var category in categories)
            {
                boxOptions.Add($"{category.Id}.{category.Name}");
            }
            var box = new MenuPlaceholder("Kategorier", 0, 0, boxOptions);
            box.Draw();
         
            Console.Write("Välj kategori: ");
            var userChoice = Console.ReadLine();
            if(int.TryParse(userChoice, out var categoryId))
            {
                if(categoryId == 0)
                {
                    RenderMainMenu();
                    return;
                }
                var category = categories.FirstOrDefault(c => c.Id == categoryId);
                if(category == null)
                {
                    ShowErrorInput(RenderCategories);
                }
                else
                {
                    RenderProducts(category.Products.ToList(), RenderCategories);
                }
            }
            else
            {
                ShowErrorInput(RenderCategories);
            }
        }

        private static void RenderProduct(Product product, Action goBackAction)
        {
            Console.Clear();
            var boxOptions = new List<string>();
            boxOptions.Add($"Beskrivning: {product.Description}");
            boxOptions.Add($"Kategori: {product.Category?.Name}");
            boxOptions.Add($"Färg: {product.Color}");
            boxOptions.Add($"Storlek: {product.Size}");
            boxOptions.Add($"");
            boxOptions.Add($"0: Gå tillbaka");
            boxOptions.Add($"1: Köp");

            var box = new MenuPlaceholder(product.Name, 0, 0, boxOptions);

            box.Draw();

            var userChoice = Console.ReadLine();

            if (int.TryParse(userChoice, out var choice))
            {
                
                 RenderProduct(product, goBackAction);
                return;
               
            }
            if (choice == 0)
            {
                goBackAction();
                return;
            }
            if (choice != 1)
            {
                RenderProduct(product, goBackAction);
                return;
            }

            Console.WriteLine("Hur många vill du köpa: ");
            var quantityInput = Console.ReadLine(); 

            if(!int.TryParse(quantityInput, out var quantity))
            {
                Console.WriteLine("Felaktigt antal, klicka enter för att försöka igen");
                Console.ReadLine();
                RenderProduct(product, goBackAction);
                return;
            }
            if(quantity <= 0)
            {
                Console.WriteLine("Felaktigt antal, klicka enter för att försöka igen");
                Console.ReadLine();
                RenderProduct(product, goBackAction);
                return;
            }

            if(_loggedInCustomer == null)
            {
                ShowErrorInput(RenderCustomerLogIn, "Du måste logga in");
                return;
            }
            if(product.StockQuantity < quantity)
            {
                Console.WriteLine("Otillräckligt lagersaldo");
                Console.ReadLine();
                RenderProduct(product, goBackAction);
                return;
            }
            var cart = _database.Carts
               .Include(c => c.Items)
               .FirstOrDefault(c => c.CustomerId == _loggedInCustomer.Id);
             
            if(cart == null)
            {
                cart = new Cart
                {
                    CustomerId = _loggedInCustomer.Id,
                };

                _database.Carts.Add(cart);
            }

            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                Quantity = quantity
            });

            _database.SaveChanges();

            Console.WriteLine($"{quantity}st {product.Name} har lagts i kundkorgen, klicka enter för att gå vidare");
            Console.ReadLine();
            goBackAction();

        }
        private static void RenderBasket()
        {
            while (true)
            {
                Console.Clear();
                var boxOptions = new List<string>();

                if(_loggedInCustomer == null)
                {
                    ShowErrorInput(RenderCustomerLogIn, "Du måste logga in");
                    return;
                }
            
                var items = _database.CartItems
                    .Include(p => p.Product)
                    .Include(c => c.Cart)
                    .Where(c => c.Cart.CustomerId == _loggedInCustomer.Id)
                    .ToList();

                if (!items.Any())
                {
                    boxOptions.Add("Din kundkorg är tom ");
                }
                else
                {
                    decimal total = 0;
                    for(int i = 0; i < items.Count; i++)
                    {
                        var cartItem = items[i];
                        var itemTotal = cartItem.Quantity * cartItem.Product.Price;
                        boxOptions.Add($"{cartItem.ProductId}.{cartItem.Product.Name} -{cartItem.Quantity} st á {cartItem.Product.Price:C2}, {itemTotal:C2}");
                        total += itemTotal;
                    }
                    boxOptions.Add("");
                    boxOptions.Add($"Totalt: {total:C2}");
                }
                var box = new MenuPlaceholder("Kundkorg", 0, 0, boxOptions);
                box.Draw();

                var menuOptions = new List<string>();
                menuOptions.Add("0. Tillbaka till huvudmenyn");
                menuOptions.Add("A. Töm hela kundkorgen");
                menuOptions.Add("B. Gå till betalning");
                menuOptions.Add("(ProduktId).Radera enskild produkt");

                var optionBox = new MenuPlaceholder("Dina alternativ", 0, items.Count * 5, menuOptions);
                optionBox.Draw();
               
                var input = Console.ReadLine();

                switch( input.ToLower() )
                {
                    case "0":
                        RenderMainMenu();
                        return;
                    case "a":
                        _database.RemoveRange(items);
                        _database.SaveChanges();
                        Console.WriteLine("Kundkorgen är tömd"); 
                        Console.ReadLine();
                        RenderBasket();
                        break;
                    case "b":
                        if (!items.Any())
                        {
                            ShowErrorInput(RenderBasket, "Kundkorgen är tom");
                            return;
                        }
                        RenderShipping();
                        return;
                    default:

                        if(int.TryParse(input, out var productId))
                        {
                            var item = items.Where(x => x.ProductId == productId).FirstOrDefault();
                            if (item != null)
                            {
                                _database.Remove(item);
                                _database.SaveChanges();
                                Console.WriteLine($"{item.Product.Name} är raderad"); // Fixa så man kan välja produkt att ta bort inte bara tömma hela kundkorgen
                                Console.ReadLine();
                                RenderBasket();
                            }
                        }
                        else
                        {
                            ShowErrorInput(RenderBasket);
                        }
                          
                        return;
                }
            }

        }

        private static void RenderSearch()
        {
            Console.Clear();
            var menuOptions = new List<string>();
            menuOptions.Add("0. Tillbaka till huvudmenyn");
            menuOptions.Add("Valfritt sökord");
           
            var optionBox = new MenuPlaceholder("Sök efter produkt", 0, 0, menuOptions);
            optionBox.Draw();

            var input = Console.ReadLine();

            if(input == "0")
            {
                RenderMainMenu();
                return;
            }
           
                var query = """
                    SELECT *
                    FROM Products
                    WHERE Name LIKE @search OR Description LIKE @search
                    """;

                try
                {
                    using (var conn = new SqlConnection("Server=.\\SQLExpress;Database=LilosShop;Trusted_Connection=True; TrustServerCertificate=True;"))
                    {
                        var products = conn.Query<Product>(
                            query,
                            new {search = $"%{input}%"}
                            ).ToList();
                    }
                }
                catch (Exception e)
                {
                    ShowErrorInput(RenderSearch, e.Message);
                }
            

        }

        private static void RegisterCustomer()
        {
            Console.Clear ();

            Console.WriteLine("Registrera ny kund");

            Console.WriteLine("Namn:");
            var name = Console.ReadLine();

            Console.WriteLine("Email: ");
            var email = Console.ReadLine();

            Console.WriteLine("Adress: ");
            var address = Console.ReadLine();

            Console.WriteLine("Ort: ");
            var city = Console.ReadLine();

            Console.WriteLine("Telefonnummer: ");
            var phone = Console.ReadLine();

            Console.WriteLine("Välj lösen: ");
            var userPassword = Console.ReadLine();

            using var db = new Database();

            var customer = db.Customers
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.Email == email && c.Password == userPassword);

           

        }

        private static void RenderCustomerLogIn()
        {
            Console.Clear();

            Console.WriteLine("Kund-login");

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Lösenord: ");
            var password = Console.ReadLine();  

            var customer = _database.Customers
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.Email == email && c.Password == password);

            if(customer == null)
            {
                ShowErrorInput(RenderMainMenu, "Fel Inloggning, tryck enter för att försöka igen");
                Console.ReadLine();
                RenderCustomerLogIn();
                return;
            }

            _loggedInCustomer = customer;

            
            Console.WriteLine($"Inloggad som {customer.Name}");
            Console.ReadLine();
            RenderMainMenu();
        }

        private static Admin _loggedInAdmin = null;
        private static void RenderAdminLogIn()
        {
            Console.Clear();
            Console.WriteLine("Admin-login");

            Console.Write("Användarnamn:");
            var userName = Console.ReadLine();

            Console.WriteLine("Adminlösenord: ");
            var password = Console.ReadLine();
            
            var admin = _database.Admins
                .FirstOrDefault(a => a.UserName == userName && a.PassWord == password);
             
            if(admin == null)
            {
                Console.WriteLine("Fel användarnamn eller lösenord, tryck enter för att försöka igen");
                Console.ReadLine();
                RenderAdminLogIn();
                return;
            }

            _loggedInAdmin = admin;
            RenderAdminMenu();
        }

        private static void RenderAdminMenu()
        {
            if( _loggedInAdmin == null )
            {
                Console.WriteLine("Du måste vara inloggad som admin.");
                Console.ReadLine();
                RenderAdminLogIn();
                return ;
            }
            Console.Clear();
            Console.WriteLine("Adminmeny:");

            var adminOptions = new List<string>();

            adminOptions.Add("0. Tillbaka");
            adminOptions.Add("1. Se alla produkter");
            adminOptions.Add("2. Skapa produkt");
            adminOptions.Add("3. Skapa kategori");
            adminOptions.Add("4. Redigera produkt");
            adminOptions.Add("5. Redigera kategori");
            adminOptions.Add("6. Ta bort produkt");
            adminOptions.Add("7. Se kunder och redigera");
            adminOptions.Add("8. Se lagersaldo och leverantör");

            var adminBox = new MenuPlaceholder("Admin", 0, 0, adminOptions);
            adminBox.Draw();

            var input = Console.ReadLine();

            switch(input)
            {
                case "0":
                    _loggedInAdmin = null;
                    RenderMainMenu();
                    break;
                case "1": 
                    RenderAllProducts(); 
                    break;
                case "2":
                    RenderCreateProduct();
                    break;
                case "3":
                    RenderCreateCategory();
                    break;
                case "4":
                    RenderEditProduct();
                    break;
                case "5":
                    RenderEditCategory();
                    break;
                case "6":
                    RenderDeleteProduct();
                    break;
                case "7":
                    RenderAdminCustomers();
                    break;
                case "8":
                    RenderAdminInStock();
                    break;
                default:
                    ShowErrorInput(RenderAdminMenu);
                    break;

            }

        }

        private static void RenderAdminCustomers()
        {
            Console.Clear();
            var customers = _database.Customers.ToList();

            foreach (var c in customers)
            {
                Console.WriteLine($"{c.Id}. {c.Name} | {c.Email} | {c.Address} | {c.City} | {c.Phonenumber}");
            }

            Console.WriteLine("\nVälj kundId för att redigera eller 0 för att gå tillbaka: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || id == 0)
            {
                RenderAdminMenu();
                return;
            }

            var customer = customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                ShowErrorInput(RenderAdminMenu);
                return;
            }

            Console.WriteLine($"Namn ({customer.Name})");
            var input = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(input)) customer.Name = input;

            Console.WriteLine($"Email ({customer.Email})");
            input = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(input)) customer.Email = input;

            Console.WriteLine($"Address ({customer.Address})");
            input = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(input)) customer.Address = input;

            Console.WriteLine($"Stad ({customer.City})");
            input = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(input)) customer.City = input;

            Console.WriteLine($"Telefonnummer ({customer.Phonenumber})");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                customer.Phonenumber = input;

            _database.SaveChanges();
            Console.WriteLine("Kunduppgifter uppdaterade!");
            Console.ReadLine();
            RenderAdminMenu();

        }

        private static void RenderAdminInStock()
        {
            Console.Clear();
            var products = _database.Products.ToList();

            foreach(var p in products)
            {
                Console.WriteLine($"Id: {p.Id} | {p.Name} | Lagersaldo: {p.StockQuantity} | Leverantör: {p.Supplier}");
            }

            Console.WriteLine("\nTryck enter för att återgå till adminmenyn");
            Console.ReadLine() ;
            RenderAdminMenu();
        }
        private static void RenderCreateProduct()
        {
            Console.Clear();

            Console.Write("Namn: ");
            var name = Console.ReadLine();

            Console.Write("Beskrivning: ");
            var description = Console.ReadLine();

            Console.Write("Pris: ");
            decimal.TryParse(Console.ReadLine(), out var price);

            Console.Write("Färg: ");
            Enum.TryParse(Console.ReadLine(), true, out Color color);

            Console.Write("Storlek (ange siffra): ");
            if(!int.TryParse(Console.ReadLine(),out var size))
            {
                ShowErrorInput(RenderCreateProduct, "Felaktig storlek");
                return;
            }

            Console.Write("Lagersaldo: ");
            int.TryParse(Console.ReadLine(), out var stock);
            Console.Write("Leverantör: ");
            var supplier = Console.ReadLine();

            var categories = _database.Categories.ToList();
            foreach(var c  in categories)
            {
                Console.WriteLine($"{c.Id}. {c.Name}");
            }

            Console.Write("Välj kategoriId: ");
            if(!int.TryParse(Console.ReadLine(),out var categoryId))
            {
                ShowErrorInput(RenderCreateProduct);
                return;
            }


            Console.Write("Specialerbjudande j/n: ");
            var isOffer = Console.ReadLine()?.ToLower() == "j";

            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Color = color,
                Size = size,
                CategoryId = categoryId,
                IsSpecialOffer = isOffer
            };

            _database.Products.Add(product);
            _database.SaveChanges();

            Console.WriteLine("Produkt skapad");
            Console.ReadLine();
            RenderAdminMenu();


        }

        // Redigera en produkt
        private static void RenderEditProduct()
        {
            var products = _database.Products.ToList();
            foreach(var p in products)
            {
                Console.WriteLine($"{p.Id}. {p.Name}");
            }

            Console.Write("Välj produktId: ");
            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                ShowErrorInput(RenderAdminMenu);
                return;
            }

            var product = products.FirstOrDefault(p => p.Id == id);
            if(product == null)
            {
                ShowErrorInput(RenderAdminMenu);
            }

            //Namn
            Console.WriteLine($"Nytt namn ({product.Name}) ");
            var name = Console.ReadLine();  
            if(!string.IsNullOrWhiteSpace(name))
            {
                product.Name = name;
            }

            Console.WriteLine($"Ny beskrivning ({product.Description}):  ");
            var description = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(description))
            {
                product.Description = description;
            }

            // Pris
            Console.WriteLine($"Nytt pris ({product.Price})  ");
            var priceInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(priceInput))
            {
                if(decimal.TryParse(priceInput, out var price) && price > 0)
                {
                    product.Price = price;
                }
                else
                {
                    Console.WriteLine("Felaktigt pris, nuvarande pris behålls. ");
                }
            }

            //Färg
            Console.WriteLine($"Ny färg ({product.Color}):  ");
            foreach(var value in Enum.GetValues(typeof(Color)))
            {
                Console.WriteLine($"{(int)value}. {value}");
            }
            var colorInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(colorInput))
            {
                if (int.TryParse(colorInput, out var colorValue) && Enum.IsDefined(typeof(Color), colorValue))
                {
                    product.Color = (Color)colorValue;
                }
                else
                {
                    Console.WriteLine("Felaktigt färg, nuvarande färg behålls. ");
                }
            }

            // Storlek
            Console.WriteLine($"Ny storlek ({product.Size}) ");
            var sizeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(sizeInput))
            {
                if(int.TryParse(sizeInput, out var size))
                {
                    product.Size = size;
                }
                else
                {
                    Console.WriteLine("Felktig storlek, nuvarande storlek behålls");
                }
               
            }
            // Specialerbjudande
            Console.WriteLine($"Specialerbjudande ({(product.IsSpecialOffer ? "j"  : "n")}) ");
            var offerInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(offerInput))
            {
                if (offerInput.ToLower() == "j")
                {
                    product.IsSpecialOffer = true;
                }
                else if (offerInput.ToLower() == "n")
                {
                    product.IsSpecialOffer = false;
                }

            }

            _database.SaveChanges();

            Console.WriteLine("Produkt uppdaterad ");
            Console.ReadLine ();
            RenderAdminMenu();
        }

        private static void RenderDeleteProduct()
        {
            Console.Clear();

            var products = _database.Products.ToList();
            foreach (var p in products)
            {
                Console.WriteLine($"{p.Id}. {p.Name}");
            }
            Console.WriteLine("Välj produktId att radera: ");
            if(!int.TryParse(Console.ReadLine(),out var id))
            {
                ShowErrorInput (RenderAdminMenu);
                return;
            }

            var product = products.FirstOrDefault(x => x.Id == id);
            if(product == null)
            {
                ShowErrorInput(RenderAdminMenu);
            }

            _database.Products.Remove(product);
            _database.SaveChanges ();

            Console.WriteLine($"{product} är raderad");
            Console.ReadLine ();
            RenderAdminMenu();
        }

        private static void RenderCreateCategory()
        {
            Console.Clear();

            Console.Write("Ange Kategorinamn: ");
            var name = Console.ReadLine();

            if(string.IsNullOrEmpty(name) )
            {
                ShowErrorInput(RenderAdminMenu);
                return;
            }
            if(_database.Categories.Any(c => c.Name.ToLower() == name.ToLower()))
            {
                ShowErrorInput(RenderAdminMenu, "Kategorin finns redan");
            }

            _database.Categories.Add(new Category { Name = name });
            _database.SaveChanges();

            Console.WriteLine("Kategorin skapad");
            Console.ReadLine();
            RenderAdminMenu();
            

        }

        private static void RenderEditCategory()
        {
            Console.Clear ();

            var categories = _database.Categories.ToList();
            foreach (var c in categories)
            {
                Console.WriteLine($"{c.Id}. {c.Name}");
            }
            Console.Write("Välj kategoriId");
            if(!int.TryParse(Console.ReadLine(), out var id))
            {
                ShowErrorInput (RenderAdminMenu);
                return;
            }

            var category = _database.Categories.FirstOrDefault(c => c.Id == id);    
            if (category == null)
            {
                ShowErrorInput(RenderAdminMenu);
                return;
            }

            Console.WriteLine("Ange nytt namn: ");
            category.Name = Console.ReadLine();
            _database.SaveChanges();

            Console.WriteLine("Kategorin är uppdaterad");
            Console.ReadLine();
            RenderAdminMenu();
        }

        private static void RenderShipping()
        {
            Console.Clear ();

            if(_loggedInCustomer == null)
            {
                ShowErrorInput(RenderCustomerLogIn, "Du måste logga in för att beställa");
                return;
            }

            //Hämta kundkorg för rätt kund
            var cartItems = _database.CartItems
                .Include(c => c.Product)
                .Include(c => c.Cart)
                .Where(c => c.Cart.CustomerId == _loggedInCustomer.Id)
                .ToList();

            if(!cartItems.Any())
            {
                ShowErrorInput(RenderBasket, "Din kundkorg är tom");
                return;
            }

            Console.Clear ();

            //Visar kundinformation
            Console.WriteLine($"Namn: {_loggedInCustomer.Name}");
            Console.WriteLine($"Telefonnummer: {_loggedInCustomer.Phonenumber}");
            Console.WriteLine($"Adress: {_loggedInCustomer.Address}");
            Console.WriteLine($"Ort: {_loggedInCustomer.City}");
            Console.WriteLine();

            //Frakt alternativ
            Console.WriteLine("1. Postnord (49 Kr)");
            Console.WriteLine("2. DHL (99 Kr)");

            var choice = Console.ReadLine();

            string method = choice == "2" ? "DHL" : "Postnord";
            decimal cost = choice == "2" ? 99 : 49;

            RenderPayment(method, cost);

        }
        private static void RenderPayment(string shippingMethod, decimal shippingCost)
        {
            Console.Clear ();

            if(_loggedInCustomer == null)
            {
                ShowErrorInput(RenderCustomerLogIn, "Du måste logga in för att betala");
                return;
            }

            //Hämta kundkorg för rätt kund
            var cartItems = _database.CartItems
                .Include(c => c.Product)
                .Include(c => c.Cart)
                .Where(c => c.Cart.CustomerId == _loggedInCustomer.Id)
                .ToList();

            if(!cartItems.Any())
            {
                ShowErrorInput(RenderBasket, "Din kundkorg är tom");
                return;
            }

            decimal total = shippingCost;
            foreach(var item in cartItems)
            {
                total += item.Quantity * item.Product.Price;
            }

            Console.WriteLine($"Betalning för din beställning");
            Console.WriteLine($"Frakt: {shippingMethod} ({shippingCost:C2})");
            Console.WriteLine($"Totalt att betala: {total:C2}");
            Console.WriteLine();
            Console.WriteLine("Bekräfta betalning? (J/N)");

            var input = Console.ReadLine();
            if(input?.ToLower() != "j")
            {
                RenderBasket();
                return;
            }

            foreach(var item in cartItems)
            {
                if(item.Product.StockQuantity < item.Quantity)
                {
                    ShowErrorInput(RenderBasket, $"Otillräckligt lagersaldo för {item.Product.Name}");
                    return;
                }
            }

            var order = new Order
            {
                CustomerId = _loggedInCustomer.Id,
                OrderDate = DateTime.Now,
                TotalPrice = total,
                ShippingMethod = shippingMethod
            };

            _database.Orders.Add(order);

            foreach(var item in cartItems)
            {
                _database.OrderItems.Add(new OrderItem
                {
                    Order = order,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                }); 
                  
                //Dra av lagret
                item.Product.StockQuantity -= item.Quantity;
                    
            }


            _database.CartItems.RemoveRange(cartItems);
            _database.SaveChanges();

            Console.WriteLine("Tack för din beställning! Tryck enter för att gå till huvudmenyn");
            Console.ReadLine();
            RenderMainMenu();




            //var items = _database.CartItems.Include(c => c.Product).ToList();

            //decimal subtotal = items.Sum(i => i.Product.Price * i.Quantity);
            //decimal moms = subtotal * 0.25m;
            //decimal total = subtotal + moms + shippingCost;

            //Console.WriteLine($"Varor: {subtotal:C}");
            //Console.WriteLine($"Moms: {moms:C}");
            //Console.WriteLine($"Frakt: {shippingMethod} {shippingCost:C}");
            //Console.WriteLine($"Totalt: {total:C}");
            //Console.WriteLine();

            //Console.WriteLine("1.Kort");
            //Console.WriteLine("2.Swish");

            //var paymetChoice = Console.ReadLine();
            //var paymentMethod = paymetChoice == "2" ? "Swish" : "Kort";

            //var order = new Order
            //{
            //    CustomerId = _loggedInCustomer.Id,
            //    OrderDate = DateTime.Now,
            //    ShippingMethod = shippingMethod,
            //    ShippingCost = shippingCost,
            //    PaymentMethod = paymentMethod,
            //    TotalPrice = total,
            //    Status = OrderStatus.Mottagen
            //};

            //foreach(var item in items)
            //{
            //    order.Items.Add(new OrderItem
            //    {
            //        ProductId = item.ProductId,
            //        Quantity = item.Quantity,
            //        Price = item.Product.Price,
            //        Product = item.Product
            //    });

            //    item.Product.StockQuantity -= item.Quantity;
            //}

            //_database.Orders.Add(order);
            //_database.CartItems.RemoveRange(items);
            //_database.SaveChanges();

            //Console.WriteLine("Betalningen är genomförd! ");
            //Console.ReadLine ();
            //RenderMainMenu();   
        }

        private static void RenderMyOrders()
        {
            if(_loggedInCustomer == null)
            {
                ShowErrorInput(RenderMainMenu, "Du måste logga in");
                return;
            }

            Console.Clear();
            var orders = _database.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.CustomerId == _loggedInCustomer.Id)
                .ToList();

            if(!orders.Any())
            {
                Console.WriteLine("Inga ordrar ännu ");
                Console.ReadLine();
                RenderMainMenu();
                return;
            }

            foreach(var order in orders)
            {
                Console.WriteLine($"Order - {order.Id} - {order.OrderDate}");
                Console.WriteLine($"Status: {order.Status}");
                Console.WriteLine($"Frakt: {order.ShippingMethod} ({order.ShippingCost:C})");
                Console.WriteLine($"Betalning: {order.PaymentMethod}");
                Console.WriteLine($"Total: {order.TotalPrice}");
                Console.WriteLine("Innehåll: ");

                foreach (var item in order.Items)
                {
                    Console.WriteLine($" - {item.Product.Name} x {item.Quantity} a {item.Price}");
                }

            }
            Console.WriteLine("Tryck enter för att återgå till huvudmeny");
            Console.ReadLine();
            RenderMainMenu ();
            
        }

        private static void ShowErrorInput(Action actionToRunAfterErrorIsPresented, string optionalErrorText = "")
        {
            if (!string.IsNullOrEmpty(optionalErrorText))
            {
                Console.WriteLine(optionalErrorText);
            }
            else
            {
                Console.WriteLine("Felaktig inmatning, klicka för att försöka igen");
            }

            Console.ReadLine();
            actionToRunAfterErrorIsPresented();
        }

    }
}
