using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var rpcClient = new RpcClient();
            while (true)
            {
                Console.WriteLine("输入要计算的数:");
                var msg = Console.ReadLine();
                if (msg == "Q") { break; }
                var response = rpcClient.Call(msg);
                Console.WriteLine(" 得到结果'{0}'", response);
            }
            rpcClient.Close();
        }
    }
}
