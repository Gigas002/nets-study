using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace NetsTests
{
    internal static class Program
    {
        internal static async Task Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            #region Create Uri, get host name form it, get IP adress from host name

            Uri sankakuUri = new Uri("https://www.sankakucomplex.com/");
            IPAddress iPAddress = await GetHostIpFromName(sankakuUri.Host);
            Console.WriteLine($"{iPAddress}");

            #endregion

            //Create downloads directory
            DirectoryInfo downloadsDirectoryInfo = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "downloads"));
            downloadsDirectoryInfo.Create();

            #region Create web client and download an image

            Uri imageUri = new Uri("https://www.sankakucomplex.com/wp-content/uploads/2019/07/kyoto-animation-studio-park.jpg");
            FileInfo downloadedFileInfo = new FileInfo(Path.Combine(downloadsDirectoryInfo.FullName, "file1.jpg"));
            using (WebClient webClient = new WebClient())
                await webClient.DownloadFileTaskAsync(imageUri, downloadedFileInfo.FullName);

            #endregion

            // WebRequest webRequest = WebRequest.Create(sankakuUri);
            // WebResponse webResponse = await webRequest.GetResponseAsync();
            // using (Stream stream = webResponse.GetResponseStream())
            // {
            //     using (StreamReader reader = new StreamReader(stream))
            //     {
            //         string line = string.Empty;
            //         while ((line = reader.ReadLine()) != null)
            //         {
            //             Console.WriteLine(line);
            //         }
            //     }     
            // }
            // webResponse.Close();

            HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(sankakuUri);
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync())
            {
                using (Stream stream = httpWebResponse.GetResponseStream())
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        string responseString = string.Empty;
                        while ((responseString = streamReader.ReadLine()) != null)
                        {
                            Console.WriteLine(responseString);
                        }
                    }     
                }
                httpWebResponse.Close();
            }

            //Create socket
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);




            stopwatch.Stop();
            Console.WriteLine($"Ms elapsed:{stopwatch.Elapsed.Milliseconds}");
            Console.WriteLine($"Press any key for Taras Panis.");
            Console.ReadKey();
        }

        internal static async ValueTask<IPAddress> GetHostIpFromName(string hostName) =>
                (await Dns.GetHostEntryAsync(hostName)).AddressList.FirstOrDefault();
            // IPHostEntry iPHostEntry = await Dns.GetHostEntryAsync(hostName);
            // return iPHostEntry.AddressList.FirstOrDefault();
            // foreach (IPAddress iPAddress in iPHostEntry.AddressList)//.AsParallel())//     Console.WriteLine($"{iPAddress}");
    }
}
