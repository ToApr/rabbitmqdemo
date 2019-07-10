using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelayedMessagePublisher
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

            IConnection connection = connectionFactory.CreateConnection("DelayedMessagePublisher"); //创建连接 

            IModel model = connection.CreateModel(); //创建通道 channels

            string exchangeName = "delayed_Exchange";
            string queueName = "delayed_queue";
            Dictionary<string, object> exchangeargs = new Dictionary<string, object>();
            exchangeargs.Add("x-delayed-type", ExchangeType.Fanout);
            //2.创建交换机
            model.ExchangeDeclare(exchange: exchangeName,  //交换机名称
               type: "x-delayed-message",                //交换机类型  * 匹配一个单词  # 匹配 o个或多个
               durable: true,                            //持久化的
               autoDelete: false,                        //自动删除
               arguments: exchangeargs);                         //附加参数  ，例如 备用交换机，死信交换机 关联
            Dictionary<string, object> queueArgs = new Dictionary<string, object>();
            queueArgs.Add("x-queue-mode", "lazy");
            model.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);

            model.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "", arguments: null);
            model.ConfirmSelect();
            Console.WriteLine("延迟两秒到队列");
            while (true)
            {
                Console.WriteLine("输入要发送的消息。。。");
                string msg=  Console.ReadLine();
                if (msg == "Q") break;
                byte[] body=   System.Text.UTF8Encoding.UTF8.GetBytes(msg);
                IBasicProperties basicProperties = model.CreateBasicProperties();
                basicProperties.DeliveryMode = 2;
                basicProperties.Headers = new Dictionary<string, object>();
                basicProperties.Headers.Add("x-delay", "10000");
                model.BasicPublish(exchange: exchangeName, routingKey: "", mandatory: false, basicProperties: basicProperties, body: body);
                model.WaitForConfirms();
            }
            Console.ReadLine();
            model.Close();
            connection.Close();

        }
    }
}
