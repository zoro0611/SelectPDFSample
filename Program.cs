using Newtonsoft.Json;
using SelectPdf.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace testSelectPDF_API
{
    internal class Program
    {
        static string API_KEY = "My_Api_Key";
        static string TARGET_STR = "My_Target_Url_Or_Raw_Html_String";
        public static string apiEndpoint = "https://selectpdf.com/api2/convert/";
        static void Main(string[] args)
        {
            //SelectPdfPostWithHttpWebRequest();
            //SelectPdfPostWithWebClient();
            SelectPdfByRawHtmlString();//1234 Git new branch
            Console.ReadLine();
        }

        public static void SelectPdfPostWithHttpWebRequestByUrl()
        {
            System.Console.WriteLine("Starting conversion with HttpWebRequest ...");

            // set parameters
            SelectPdfParameters parameters = new SelectPdfParameters();
            parameters.key = API_KEY;
            parameters.url = TARGET_STR;

            // JSON serialize parameters
            string jsonData = JsonConvert.SerializeObject(parameters);
            byte[] byteData = Encoding.UTF8.GetBytes(jsonData);

            // create request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiEndpoint);

            request.ContentType = "application/json";
            request.Method = "POST";
            request.Credentials = CredentialCache.DefaultCredentials;

            // POST parameters
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteData, 0, byteData.Length);
            dataStream.Close();

            // GET response (if response code is not 200 OK, a WebException is raised)
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // all ok - read PDF and write on disk (binary read!!!!)
                    MemoryStream ms = BinaryReadStream(responseStream);

                    // write to file
                    FileStream file = new FileStream($"{TARGET_STR}.pdf", FileMode.Create, FileAccess.Write);
                    ms.WriteTo(file);
                    file.Close();
                }
                else
                {
                    // error - get error message
                    System.Console.WriteLine("Error code: " + response.StatusCode.ToString());
                }
                responseStream.Close();
            }
            catch (WebException webEx)
            {
                // an error occurred
                System.Console.WriteLine("Error: " + webEx.Message);

                HttpWebResponse response = (HttpWebResponse)webEx.Response;
                Stream responseStream = response.GetResponseStream();

                // get details of the error message if available (text read!!!)
                StreamReader readStream = new StreamReader(responseStream);
                string message = readStream.ReadToEnd();
                responseStream.Close();

                System.Console.WriteLine("Error Message: " + message);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error: " + ex.Message);
            }

            System.Console.WriteLine("Finished.");
        }
        public static void SelectPdfPostWithWebClientByUrl()
        {
            System.Console.WriteLine("Starting conversion with WebClient ...");

            // set parameters
            SelectPdfParameters parameters = new SelectPdfParameters();
            parameters.key = API_KEY;
            parameters.url = TARGET_STR;

            // JSON serialize parameters
            string jsonData = JsonConvert.SerializeObject(parameters);
            byte[] byteData = Encoding.UTF8.GetBytes(jsonData);

            // create WebClient object
            WebClient webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");

            // POST parameters (if response code is not 200 OK, a WebException is raised)
            try
            {
                byte[] result = webClient.UploadData(apiEndpoint, "POST", byteData);

                // all ok - read PDF and write on disk (binary read!!!!)
                MemoryStream ms = new MemoryStream(result);

                // write to file
                //加入目前資料夾位置
                string path = System.IO.Directory.GetCurrentDirectory();
                //combine檔案名稱和path
                string fileName = System.IO.Path.Combine(path, "mytest.pdf");
                FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                ms.WriteTo(file);
                file.Close();
            }
            catch (WebException webEx)
            {
                // an error occurred
                System.Console.WriteLine("Error: " + webEx.Message);

                HttpWebResponse response = (HttpWebResponse)webEx.Response;
                Stream responseStream = response.GetResponseStream();

                // get details of the error message if available (text read!!!)
                StreamReader readStream = new StreamReader(responseStream);
                string message = readStream.ReadToEnd();
                responseStream.Close();

                System.Console.WriteLine("Error Message: " + message);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error: " + ex.Message);
            }

            System.Console.WriteLine("Finished.");
        }
        public static void SelectPdfByRawHtmlString()
        {
            System.Console.WriteLine("Starting conversion with WebClient ...");

            try
            {
                // write to file
                //加入目前資料夾位置
                string path = System.IO.Directory.GetCurrentDirectory();
                //combine檔案名稱和path
                string fileName = System.IO.Path.Combine(path, "mytest.pdf");


                HtmlToPdfClient api = new HtmlToPdfClient(API_KEY);
                api
                    .setPageSize(PageSize.A4)
                    .setPageOrientation(PageOrientation.Portrait)
                    .setMargins(10)
                    .setNavigationTimeout(30)
                    .setShowPageNumbers(false)
                    .setPageBreaksEnhancedAlgorithm(true)
                ;
                Console.WriteLine("Starting conversion ...");
                //api.convertHtmlStringToStream(TARGET_STR, );
                api.convertHtmlStringToFile(TARGET_STR, fileName);

                Console.WriteLine("Conversion finished successfully!");


                UsageClient usage = new UsageClient(API_KEY);
                UsageInformation info = usage.getUsage(false);
                Console.WriteLine("Conversions left this month: " + info.Available);

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
        public static MemoryStream BinaryReadStream(Stream input)
        {
            int bytesNumber = 0;
            byte[] bytes = new byte[1025];
            MemoryStream stream = new MemoryStream();
            BinaryReader reader = new BinaryReader(input);

            do
            {
                bytesNumber = reader.Read(bytes, 0, bytes.Length);
                if (bytesNumber > 0)
                    stream.Write(bytes, 0, bytesNumber);
            } while (bytesNumber > 0);

            stream.Position = 0;
            return stream;
        }
    }
    // API parameters - add the rest here if needed
    public class SelectPdfParameters
    {
        public string key { get; set; }
        public string url { get; set; }
        public string html { get; set; }
        public string base_url { get; set; }
    }
}
