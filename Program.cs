using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleTables;
using Pastel;

namespace ApiClient
{
    class Program
    {

        static async Task AddOneItem(string token, Item newItem)
        {
            var client = new HttpClient();

            // CREATE
            // Generate a URL specifically referencing the endpoint for getting a single
            // todo item and provide the id supplied
            var url = $"http://one-list-api.herokuapp.com/items?access_token={token}";

            // Take the 'newItem' and serialize it into JSON
            var jsonBody = JsonSerializer.Serialize(newItem);

            // turn into a StringContent object and indicated JSON is being used
            // by ensuring there is a media type header of 'application/json'
            var jsonBodyAsContent = new StringContent(jsonBody);
            jsonBodyAsContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // RESPONSE
            // Send the POST request to the URL and supply the JSON body
            var response = await client.PostAsync(url, jsonBodyAsContent);

            // Get the response as a stream
            var responseJson = await response.Content.ReadAsStreamAsync();

            // Supply the *stream of data* to a Deserializer that will interpret it as a *SINGLE* 'Item'
            var item = await JsonSerializer.DeserializeAsync<Item>(responseJson);

            // Make a table to output new item
            var table = new ConsoleTable("ID", "Description", "Created At", "Updated At", "Completed Status");

            // Add one row to table
            table.AddRow(item.Id, item.Text, item.CreatedAt, item.UpdatedAt, item.CompletedStatus);

            // Write the table
            table.Write(Format.Minimal);
        }

        static async Task GetOneItem(string token, int id)
        {
            try
            {
                var client = new HttpClient();

                var responseBodyAsStream = await client.GetStreamAsync($"http://one-list-api.herokuapp.com/items/{id}?access_token={token}");

                // Use JsonSerializer to *DE*serialize the stream into a nice List of Item objects!
                //         Describe the Shape of the data (Object in JSON => Item)
                //                                                 V
                var item = await JsonSerializer.DeserializeAsync<Item>(responseBodyAsStream);


                var table = new ConsoleTable("Description", "Created At", "Completed Status");

                table.AddRow(item.Text, item.CreatedAt, item.CompletedStatus);

                table.Write();
            }

            // Be as specific as possible with Exceptions
            catch (HttpRequestException)
            {
                Console.WriteLine("I could not find that item.");
            }
            catch (IndexOutOfRangeException)
            {
                // Do something else
            }
        }

        static async Task ShowAllItems(string token)
        {
            try
            {
                var client = new HttpClient();

                var responseBodyAsStream = await client.GetStreamAsync($"http://one-list-api.herokuapp.com/items?access_token={token}");

                // Use JsonSerializer to *DE*serialize the stream into a nice List of Item objects!
                //         Describe the Shape of the data (array in JSON => List, Object in JSON => Item)
                //                                                      |    |
                //                                                      V    V
                var items = await JsonSerializer.DeserializeAsync<List<Item>>(responseBodyAsStream);

                var table = new ConsoleTable("Description", "Created At", "Completed Status");

                // Bak in the world of List/LINQ/C#
                foreach (var item in items)
                {
                    table.AddRow(item.Text, item.CreatedAt, item.CompletedStatus);//.Pastel(Color.FromArgb(165, 229, 250)));
                }

                // Console.WriteLine($"here is {"ENTER".Pastel(Color.FromArgb(165, 229, 250))}");
                table.Write();
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("That list does not exist.");
            }
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
                Console.Write("Get (A)ll to-do, (C)reate an item, (O)ne to-do, or (Q)uit: ");
                var choice = Console.ReadLine().ToUpper();

                switch (choice)
                {
                    case "Q":
                        keepGoing = false;
                        break;

                    case "C":
                        Console.Write("Enter the description of your new todo: ");
                        var text = Console.ReadLine();

                        var newItem = new Item
                        {
                            Text = text
                        };

                        await AddOneItem(token, newItem);

                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                        break;

                    case "O":
                        Console.Write("Enter the ID ");

                        var id = int.Parse(Console.ReadLine());

                        await GetOneItem(token, id);
                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
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
