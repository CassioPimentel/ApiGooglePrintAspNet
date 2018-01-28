using GoogleCloudPrintApi;
using GoogleCloudPrintApi.Models.Application;
using GoogleCloudPrintApi.Models.Printer;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Web.Mvc;

namespace ApiGoolePrint.Controllers
{
    public class HomeController : Controller
    {
        private const string TokenPath = "C:/Users/Cassio/Desktop/token.txt";

        private const string ClientId = "";
        private const string ClientSecret = "";
        private const string ProxyPath = "proxy.txt";
        private const string TicketFolderPath = "ticket";
        private const string DocumentFolderPath = "document";
        private static readonly GoogleCloudPrintOAuth2Provider provider = new GoogleCloudPrintOAuth2Provider(ClientId, ClientSecret);
        private static GoogleCloudPrintApi.Models.Token token = null;
        private static string proxy = null;

        public ActionResult Index()
        {
            token = GenerateAndSaveToken();

            return View();
        }

        private static GoogleCloudPrintApi.Models.Token GenerateAndSaveToken()
        {
            var token2 = System.IO.File.ReadAllText(TokenPath);

            if (token2 == "")
            {
                var url = provider.BuildAuthorizationUrl("http://127.0.0.1");

                var process = Process.Start("C:/Program Files (x86)/Google/Chrome/Application/chrome.exe", url);

                var token = provider.GenerateRefreshTokenAsync("4/h0LryklYY1ByxehWfgjZYyHGO1X_Y5vzFC18yoATZ-0", "http://127.0.0.1").Result;
                SaveToken(token);
            }

            dynamic token3 = JsonConvert.DeserializeObject(token2);

            DateTime dataExpiracao = token3.expire_datetime;

            if (DateTime.Now > dataExpiracao)
            {
                var token = provider.GenerateAccessTokenAsync((string)token3.refresh_token).Result;
                SaveToken(token);
            }
            else
            {
                token = new GoogleCloudPrintApi.Models.Token((string)token3.access_token, 
                                                             (string)token3.token_type, 
                                                             (long)token3.expires_in, 
                                                             (string)token3.refresh_token, 
                                                             (DateTime?)token3.expire_datetime);
            }

            return token;
        }

        public void testeAsync()
        {
            var client = new GoogleCloudPrintClient(provider, token);


            var request1 = new ListRequest { Proxy = "db9ab847-832e-4a0c-ab76-3673be68e9c1" };
            var response1 = client.ListPrinterAsync(request1).Result;

            //SubmitPrint(client);

            PrintJob(client);
        }

        public void SubmitPrint(GoogleCloudPrintClient client)
        {
            // Create a cloud job ticket first, it contains the printer setting of the document
            var cjt = new CloudJobTicket
            {
                Print = new PrintTicketSection
                {
                    Color = new ColorTicketItem { Type = Color.Type.STANDARD_MONOCHROME },
                    Duplex = new DuplexTicketItem { Type = Duplex.Type.LONG_EDGE },
                    PageOrientation = new PageOrientationTicketItem { Type = PageOrientation.Type.LANDSCAPE },
                    Copies = new CopiesTicketItem { Copies = 1 }
                }
            };

            // Create a request for file submission, you can either submit a url with SubmitFileLink class, or a local file with SubmitFileStream class
            var request = new SubmitRequest
            {
                PrinterId = "d08832da-03e4-a068-885b-46ea9750f3eb",
                Title = "teste",
                Ticket = cjt,
                Content = new SubmitFileLink("https://image.slidesharecdn.com/teste-140108070830-phpapp02/95/folha-de-teste-1-638.jpg?cb=1389164960") // or new SubmitFileStream(contentType, fileName, fileStream)
            };

            // Submit request
            var response = client.SubmitJobAsync(request).Result;
        }

        private static void SaveToken(GoogleCloudPrintApi.Models.Token token)
        {
            string tokenString = JsonConvert.SerializeObject(token);
            System.IO.File.WriteAllText(TokenPath, tokenString);
        }

        public void PrintJob(GoogleCloudPrintClient client)
        {
            // Create a request to list jobs of a printer
            var listRequest = new JobListRequest
            {
                PrinterId = "d08832da-03e4-a068-885b-46ea9750f3eb",
                Status = "QUEUED"
            };

            // Submit request
            var response = client.ListJobAsync(listRequest).Result;
        }

        public ActionResult About()
        {
            testeAsync();

            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}