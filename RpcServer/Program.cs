using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //1.连接到 mq

            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = "10.2.56.245";
            connectionFactory.Port = 5672; //默认端口
            connectionFactory.VirtualHost = "/";
            connectionFactory.UserName = "hw";
            connectionFactory.Password = "hw";
            connectionFactory.AutomaticRecoveryEnabled = true;

            IConnection connection = connectionFactory.CreateConnection("RpcServer"); //创建连接 

            IModel model = connection.CreateModel(); //创建通道 channels

            string queueName = "rpc_queue";

            //创建队列
            model.QueueDeclare(queue: queueName,
                   durable: true,
                   exclusive: false,           //队列是否是排他的
                   autoDelete: false,
                   arguments: null);
            model.BasicQos(0, 1, false);

            Console.WriteLine(" [x] Awaiting RPC requests ");

            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (models, ea) =>
            {
                try
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" 收到消息 {0}", message);
                    int i = 0;
                    int.TryParse(message, out i);
                    int res = fit(i);
                    Console.WriteLine(" 计算结果为 {0}", res.ToString());

                    //--计算结果相应给Client
                    IBasicProperties basicProperties = model.CreateBasicProperties();
                    basicProperties.DeliveryMode = 2;
                    basicProperties.CorrelationId = ea.BasicProperties.CorrelationId;

                    model.BasicPublish("", ea.BasicProperties.ReplyTo, basicProperties, System.Text.UTF8Encoding.UTF8.GetBytes(res.ToString()));

                    Console.WriteLine(" 计算结果为 {0},已响应给client", res.ToString());
                    //------end----
                    model.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception)
                {
                    model.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            model.BasicConsume(queueName, false, consumer);
            Console.WriteLine(" 按任意见三次结束程序！");
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            model.Close();
            connection.Close();
        }

        static int fit(int n)
        {
            return n = n + 2;
        }
    }
}
