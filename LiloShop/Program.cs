using LiloShop.Models;

namespace LiloShop
{
    internal class Program
    {
        private static List<Product> _basket = new List<Product>();
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
                "2.Shop",
                "3.Kundkorg",
                "4.Sök"
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
                        RenderShop();
                        break;
                    case 3:
                       RenderBasket();
                        break;
                    case 4:
                        Console.WriteLine("Kommer till sök");
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
            Console.WriteLine("Välkommen till Lilo");

            //todo: hämta erbjudande från databasen eller visa 3 hårdkodade
            var products = _database.Products.Take(3).ToList();

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
             
              
                var offerBox = new MenuPlaceholder($"Erbjudande {i+1}", left, 2, offerInfo);
                offerBox.Draw();
            }

            Console.WriteLine("Välj erbjudande eller klicka enter för att komma till huvudmenyn");

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
                        Console.WriteLine("användaren vill köpa erbjudande a");
                        break;
                    case "b":
                        Console.WriteLine("användaren vill köpa erbjudande b");
                        break;
                    default:
                        ShowErrorInput(RenderStartpage, "Du har angivet ett felaktigt erbjudande, försök igen");
                        break;
                }
                //todo: Här ksa jag se vilket erbjudande någon vill köpa
                //Om man anger ett felaktigt erbjudande ska varning visas och ladda om startpage
            }
            
        }

        private static void RenderShop()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Välkommen till Shoppen");

                var products = _database.Products.ToList();

                for(int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    Console.WriteLine($"{i + 1}. {product.Name} - {product.Price:C2}");
                }
                Console.WriteLine("\nVälj produktnummer för att köpa: ");
                Console.WriteLine("0: Tillbaka till huvudmeny");

                var input = Console.ReadLine();

               if(int.TryParse(input,out var choice))
                {
                    if(choice == 0)
                    {
                        RenderMainMenu();
                        return;
                    }
                    if(choice > 0 && choice <= products.Count)
                    {
                        var selectedProduct = products[choice - 1];
                        Console.WriteLine($"Du valde {selectedProduct.Name}");
                        _basket.Add( selectedProduct );
                        Console.WriteLine($"{selectedProduct} lades till i din kundkorg "); 
                        Console.ReadLine();
                        RenderMainMenu();

                    }
                    else
                    {
                        ShowErrorInput(RenderShop);
                        return;
                    }
                    
                }
                else
                {
                    ShowErrorInput(RenderShop);
                    return;
                }
            }
        }
        private static void RenderBasket()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Kungkorg");

                if (!_basket.Any())
                {
                    Console.WriteLine("Din kundkorg är tom ");
                }
                else
                {
                    decimal total = 0;
                    for(int i = 0; i < _basket.Count; i++)
                    {
                        var product = _basket[i];
                        Console.WriteLine($"{i + 1}.{product.Name} - {product.Price:C2}");
                        total += product.Price;
                    }
                    Console.WriteLine($"\nTotalt: {total:C2}");
                }
                Console.WriteLine("\n1. Tillbaka till huvudmenyn");
                Console.WriteLine("\n2. Töm kundkorgen");

               var input = Console.ReadLine();

                switch( input )
                {
                    case "1":
                        RenderMainMenu();
                        return;
                    case "2":
                        _basket.Clear();
                        Console.WriteLine("Kundkorgen är tömd"); // Fixa så man kan välja produkt att ta bort inte bara tömma hela kundkorgen
                        Console.ReadLine();
                        break;
                    default:
                        ShowErrorInput(RenderBasket);
                        return;
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
