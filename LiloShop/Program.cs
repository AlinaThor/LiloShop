using Dapper;
using LiloShop.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;

namespace LiloShop
{
    internal class Program
    {
        private static Database _database = new Database(); 

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
                "5.Sök"
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

            var products = _database.Products.Where(x => x.IsSpecialOffer)
                .Include(p => p.Category)
                .Take(3)
                .ToList();

            for(var i = 0; i < products.Count; i++)
            {
                var left = 2;
                var product = products[i];


                var offerInfo = new List<string>();
                offerInfo.Add(product.Name);
                offerInfo.Add("Kategori: " + product.Category?.Name); //Visar inte kategori i consolen - fixa
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
                //todo: Här ksa jag se vilket erbjudande någon vill köpa
                //Om man anger ett felaktigt erbjudande ska varning visas och ladda om startpage
            }
            
        }

        private static void RenderAllProducts()
        {
            Console.Clear();

            var products = _database.Products.ToList();
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
            Console.Write("Välj produktnummer för att köpa: ");


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
                if (choice == 0)
                {
                    goBackAction();
                }
                if (choice == 1 )
                {
                    Console.Write("Hur många vill du köpa: ");
                    var quantityInput = Console.ReadLine();
                    if(int.TryParse(quantityInput, out var quantity))
                    {
                        if (quantity > 0) 
                        {
                            var cart = _database.Carts.FirstOrDefault();
                            if(cart == null)
                            {
                                cart = new Cart();
                                _database.Carts.Add(cart);
                                _database.SaveChanges();
                            }
                            cart.Items.Add(new CartItem
                            {
                                Product = product,
                                ProductId = product.Id,
                                Quantity = quantity
                            });
                            _database.SaveChanges();
                            Console.WriteLine($"{quantity}st {product.Name} har lagts i kundkorgen, klicka enter för att gå vidare");
                            Console.ReadLine();
                            goBackAction();
                        }
                        else
                        {
                            Console.WriteLine("Felaktigt antal, klicka enter för att försöka igen");
                            Console.ReadLine();
                            RenderProduct(product, goBackAction);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Felaktigt antal, klicka enter för att försöka igen");
                        Console.ReadLine();
                        RenderProduct(product, goBackAction);
                    }

                }
                else
                {
                    RenderProduct(product, goBackAction);
                  
                }

            }
            else
            {
                RenderProduct(product, goBackAction);
                
            }
        }
        private static void RenderBasket()
        {
            while (true)
            {
                Console.Clear();
                var boxOptions = new List<string>();
            
              

                var items = _database.CartItems.Include(p => p.Product).ToList();

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

            if(int.TryParse(input, out var number))
            {
                RenderMainMenu();
            }
            else
            {
                var query = $"""
                SELECT *
                FROM Products WHERE Name LIKE '%{input}%' OR Description LIKE '%{input}%'
                """;

                using (var conn = new SqlConnection("Server=.\\SQLExpress;Database=LilosShop;Trusted_Connection=True; TrustServerCertificate=True;"))
                {
                    var products =  conn.Query<Product>(query).ToList();
                    RenderProducts(products, RenderSearch);
                }
            }

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
