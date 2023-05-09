using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleTables;
using Pastel;

namespace ApiClient
{
    class Program
    {

        static async Task ShowAllItems(string token)
        {
            var client = new HttpClient();

            var responseBodyAsStream = await client.GetStreamAsync($"http://one-list-api.herokuapp.com/items?access_token={token}");

            // Use JsonSerializer to *DE*serialize the stream into a nice List of Item objects!
            //         Describe the Shape of the data (array in JSON => List, Object in JSON => Item)
            //                                                      |    |
            //                                                      V    V
            var items = await JsonSerializer.DeserializeAsync<List<Item>>(responseBodyAsStream);

            var table = new ConsoleTable("Description", "Created At", "Completed");

            // Bak in the world of List/LINQ/C#
            foreach (var item in items)
            {
                table.AddRow(item.Text, item.CreatedAt, item.CompletedStatus.Pastel(Color.FromArgb(165, 229, 250)));
            }

            Console.WriteLine($"here is {"ENTER".Pastel(Color.FromArgb(165, 229, 250))}");


            table.Write();
        }


        static async Task Main(string[] args)
        {

            var token = "";

            if (args.Length == 0)
            {
                Console.WriteLine("What list would you like? ");
                token = Console.ReadLine();

            }
            else
            {
                token = args[0];
            }

            var keepGoing = true;
            while (keepGoing)
            {
                Console.Clear();
                Console.Write("Get (A)ll to-do, or (Q)uit: ");
                var choice = Console.ReadLine().ToUpper();

                switch (choice)
                {
                    case "Q":
                        keepGoing = false;
                        break;

                    case "A":
                        await ShowAllItems(token);

                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
