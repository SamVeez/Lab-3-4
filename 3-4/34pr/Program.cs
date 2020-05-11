using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using static System.String;

namespace HttpServer
{
    internal class Server
    {
        private static HttpListener listener;
        private const string Url = "http://localhost:8000/";
        // я задолбался на каждом пк пути менять 
        private const string RootPath = @"D:\Prakticheskie\3-4\34pr\";
        // САМЫЕ оригинальные данные для входа. хоть бы не взломали
        private const string Login = "test";
        private const string Password = "112";

        private static void Main()
        {
            Process.Start("http://localhost:8000/"); // старт браузераe
            while (true)
            {
                listener = new HttpListener();
                listener.Prefixes.Add(Url);
                listener.Start();
                Console.WriteLine($"если браузер не открылся, го сюда -> -> -> {Url}");

                    var listenTask = HandleIncomingConnections();
                    listenTask.GetAwaiter().GetResult();

                listener.Close();
            }
        }

        private static async Task HandleIncomingConnections()
        {
            var runServer = true;

            while (runServer)
            {
                var ctx = await listener.GetContextAsync();
                var req = ctx.Request;
                var resp = ctx.Response;

                var data = new byte[] { };
                Console.WriteLine(req.Url.AbsolutePath);
                //гет запрос "домой" если неугадал логин пароль
                if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/")
                {
                    using (var reader = new StreamReader(RootPath + "index.html"))
                    {
                        data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                    }
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;
                    Console.WriteLine("Логин/пароль гони");
                }


                //гет запрос на картиночку. как и остальной хлам валается в папке проекта
                //чекнуть можно  тут -> http://localhost:8000/img
                // P.S. там годный мемасик
                if (req.HttpMethod == "GET" && req.Url.AbsolutePath.EndsWith("jpg"))
                    if( File.Exists(RootPath + req.Url.AbsolutePath))
                {
                    data = File.ReadAllBytes(RootPath + req.Url.AbsolutePath);

                    resp.ContentType = "image/jpg";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;
                }

                //пост запрос висит на кнопочке войте
                if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/login")
                {
                    var login = Empty;
                    var password = Empty;
                    using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                    {
                        var postBody = reader.ReadToEnd().Split('&');
                        foreach (var str in postBody)
                        {
                            if (str.Contains("login"))
                            {
                                login = str.Replace("login=", Empty);
                            }

                            if (str.Contains("pass"))
                            {
                                password = str.Replace("pass=", Empty);
                            }
                        }
                    }

                    if (Login == login && Password == password)
                    {
                        using (var reader = new StreamReader(RootPath + "success.html"))
                        {
                            data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                        }
                        resp.ContentType = "text/html";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;
                        //runServer = false; закомментил шобы не закрывалась сразу, а то неуспеваю . 
                        Console.WriteLine("Ура, брутфорс сила, можешь еще поугадывать, но тут один аккаунт...");
                    }
                    else
                    {
                        using (var reader = new StreamReader(RootPath + "failure.html"))
                        {
                            data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                        }
                        resp.ContentType = "text/html";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;
                        Console.WriteLine("Ошибочка входа. нука повтори. (взломщик ухАди)");
                    }
                }

                if (data.Length.Equals(0))
                {
                    data = Encoding.UTF8.GetBytes("file not found");
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;
                    resp.StatusCode = 400;
                }
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
               
            }
        }
    }
}