using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectConsumer
{
    class Program
    {
        static void Main(string[] args)
        {

            //1.连接到 mq

            string hostname = System.Configuration.ConfigurationManager.AppSettings["hostname"];
            string queueName = System.Configuration.ConfigurationManager.AppSettings["queuename"];
            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = hostname;
            connectionFactory.Port = 5672; //默认端口
            connectionFactory.VirtualHost = "/";
            connectionFactory.UserName = "hw";
            connectionFactory.Password = "hw";
            connectionFactory.AutomaticRecoveryEnabled = true;

            IConnection connection = connectionFactory.CreateConnection("Worker" + queueName); //创建连接 
            IModel model = connection.CreateModel(); //创建通道 channels

            model.BasicQos(0, 1, false); // 客户端最大能"保持"的未确认的消息数     


            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (models, ea) =>
            {
                try
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" 收到消息 {0}", message);
                    model.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception)
                {
                    model.BasicNack(ea.DeliveryTag, false,true);
                }
            };


            model.BasicConsume(queue: queueName,   //队列的名称
                autoAck: false,                    //设置是否自动确认。建议设成false ，即不自动确认
                consumerTag: queueName + "001",    //消费者标签，用来区分多个消费者
                noLocal: false,                   //设置为true 则表示不能将同一个Connectio口中生产者发送的消息传送给这个Connection 中的消费者:
                exclusive: false,                  //设置是否排他:
                arguments: null,                   //设置消费者的其他参数:
                consumer: consumer  //设置消费者的回调函数。用来处理Rabb itMQ 推送过来的消息，比如 DefaultConsumer ， 使用时需要客户端重写(override) 其中的方法。
                );

            Console.WriteLine("准备就绪，开始接收消息。。。");
            Console.ReadLine();
            Console.ReadLine();
            string str3 = Console.ReadLine();
            model.Close();
            connection.Close();

        }
    }
}
