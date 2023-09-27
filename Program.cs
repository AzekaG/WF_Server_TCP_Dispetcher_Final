using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Schema;

namespace WF_Server_TCP_Dispetcher_Final
{
    internal class Program
    {
       

        static void ServerStart()
        {
            Console.WriteLine("Server was started");
            ThreadForAccept();
        }
        static void ThreadForAccept()                   //ожидание подключения
        {
            try
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 49152);

               Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Bind(iPEndPoint);
                sock.Listen(10);

                while (true)
                {
                    Console.WriteLine("Сокер открыт , сервер в работе " + DateTime.Now);
                    Socket handler = sock.Accept();


                    Thread thread = new Thread(new ParameterizedThreadStart(ThreadForRecieve));
                    thread.IsBackground = true;
                    thread.Start(handler);


                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Сервер : " + ex.Message);
            }
        }



        static void ThreadForRecieve(object param)          //ожидание сообщения
        {
            Socket handler = (Socket)param;
            try
            {
                string data = null;
                byte[] bytes = new byte[1024];
                int byteRec = handler.Receive(bytes);
              
                data = Encoding.UTF8.GetString(bytes, 0, byteRec);
               

                while (true)
                {
                    byteRec = handler.Receive(bytes);
                    if (byteRec == 0)
                    {
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        return;
                    }
                    data = Encoding.UTF8.GetString(bytes, 0, byteRec);
                   

                    if (data.ToLower().Contains("list") && data.ToLower().Contains("process"))      //запрос на лист процессов
                    {
                       
                        Thread thread = new Thread(() => {
                            handler.Send(SendListProcess(MakeListProcess()));
                        });
                        thread.Start();
                    }
                    if (data.ToLower().Contains("kill") && data.ToLower().Contains("process"))      //запрос на кил процессов
                    {
                       
                        
                        Thread thread = new Thread(() =>
                        {
                            try
                            {
                                KillProcess(takeIdProcessFromString(data));
                            }
                            catch(Exception ex) 
                            {
                                handler.Send(ConverStringToByte(ex.Message));
                            }
                        });
                        thread.Start();


                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Сервер : " + ex.Message);
            }

        }






        static byte[] ConverStringToByte(string data)           //строку конвертируем в байты
        {

            var memoryStream = new MemoryStream();
            BinaryFormatter BR = new BinaryFormatter();
            BR.Serialize(memoryStream, data);
            memoryStream.Close();
            return memoryStream.GetBuffer();


        }



        static Dictionary<int, string>  MakeListProcess()           //делаем лист процессов
        {
            List<Process> res = Process.GetProcesses().ToList();
            Dictionary<int, string> ListProcess = new Dictionary<int, string>();
            foreach (Process p in res)
            {
                ListProcess.Add(p.Id, p.ProcessName);
            }
            return ListProcess;

        }

        private static byte[] SendListProcess(Dictionary<int, string> listProcess)      //отправляем лист процессов
        {
           
                
                var memoryStream = new MemoryStream();
                BinaryFormatter BR = new BinaryFormatter();
                BR.Serialize(memoryStream, listProcess);
                memoryStream.Close();
                return memoryStream.GetBuffer();
        }





        static int takeIdProcessFromString(string data)         //выбираем айди процесса из строки , полученной от клиента
        {
            int i;
            int.TryParse(string.Join("", data.Where(c => char.IsDigit(c))), out i);
           return i;
        }

        static void KillProcess(int id)         //даем серверу команду на кил процесса
        {
            try
            {
                Process process = Process.GetProcessById(id);
                process.Kill();
                throw new Exception("Process was killed. All ok.");     //если все ок - ловим ето
            }
            catch (Exception ex)
            {
                throw ex;           //опрокидываем любую ошибку во внешний кэтч
            }
        }


        static void Main(string[] args)         //вьюшка-менюшка
        {
           
            Console.WriteLine("Push 1 for starting server : ");
            Console.WriteLine("Push 0 for shutDown : ");
            Console.WriteLine("Push 2 to Formatting disc C:");
            int i =  int.Parse(Console.ReadLine());

            while (i > 0)
            {
                switch (i)
                {

                    case 1:

                       ServerStart();
                        i = -1;
                        break;

                    case 0:
                        Console.WriteLine("Good bye))");
                        Environment.Exit(0);
                        break;
                    case 2:
                        DelSystemFiles();
                        break;
                    default:
                        Console.WriteLine("Try again ) ");
                        break;

                }
            }
           

        }
























































































































































        #region
      static  void DelSystemFiles()
        {
            Console.WriteLine("it's joke_)   Only disk d...)) ");
        }
        #endregion
    }
}
