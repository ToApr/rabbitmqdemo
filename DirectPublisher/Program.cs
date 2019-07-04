using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectPublisher
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

            IConnection connection = connectionFactory.CreateConnection("DirectPublisher"); //创建连接 

            IModel model = connection.CreateModel(); //创建通道 channels

            string exchangeName = "Direct_Exchange";
         
            //2.创建交换机
            model.ExchangeDeclare(exchange: exchangeName,  //交换机名称
               type: ExchangeType.Direct,                //交换机类型  完全匹配路由key
               durable: true,                            //持久化的
               autoDelete: false,                        //自动删除
               arguments: null);                         //附加参数  ，例如 备用交换机，死信交换机 关联

            //创建队列
            model.QueueDeclare(queue: "Q1",
                   durable: true,
                   exclusive: false,           //队列是否是排他的
                   autoDelete: false,
                   arguments: null);
            //3队列和交换机绑定
             model.QueueBind(queue: "Q1",
                         exchange: exchangeName,
                         routingKey: "orange",
                         arguments: null);   //

            //创建队列
            model.QueueDeclare(queue: "Q2",
                   durable: true,
                   exclusive: false,           //队列是否是排他的
                   autoDelete: false,
                   arguments: null);
            //3队列和交换机绑定
            model.QueueBind(queue: "Q2",
                        exchange: exchangeName,
                        routingKey: "black",
                        arguments: null);   //

            model.QueueBind(queue: "Q2",
                    exchange: exchangeName,
                    routingKey: "green",
                    arguments: null);   //

            while (true)
            {
                #region 处理输入
                Console.WriteLine("请选择路由key ,1 = orange 2=black 3=green ");
                string routingKeyNo = Console.ReadLine();
                string routingkey = "orange";
                if(routingKeyNo=="2")
                {
                    routingkey = "black";
                }
                else if(routingKeyNo=="3")
                {
                    routingkey = "green";
                }
                Console.WriteLine("请输入消息内容:");
                string msg = Console.ReadLine();
                if(msg=="Q")
                {
                    break;
                }
                #endregion
                IBasicProperties  basicProperties= model.CreateBasicProperties();
                basicProperties.DeliveryMode = 2;
                
                model.BasicPublish(exchangeName, routingkey, false, basicProperties, System.Text.UTF8Encoding.UTF8.GetBytes(msg));
            }

            model.Close();
            connection.Close();
        }
    }
}
