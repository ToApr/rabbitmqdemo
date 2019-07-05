/*
 * 发送订单消息，不用的Worker同干不同的活
 * 
 * */

using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderPublisher
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

            try
            {

          
            IConnection connection = connectionFactory.CreateConnection("OrderPublisher"); //创建连接 

            IModel model = connection.CreateModel(); //创建通道 channels

            string exchangeName = "Order_Exchange";
            List<string> queueNameList = new List<string>() { "Phone_Order_NoticeQueue", "Email_Order_NoticeQueue", "WX_Order_NoticeQueue" };
            //2.创建交换机
             model.ExchangeDeclare(exchange:exchangeName,  //交换机名称
                type: ExchangeType.Fanout,                //交换机类型  扇出
                durable: true,                            //持久化的
                autoDelete: false,                        //自动删除
                arguments: null);                         //附加参数  ，例如 备用交换机，死信交换机 关联
       
            foreach (string queuename in queueNameList)
            {
                model.QueueDeclare(queue: queuename, 
                    durable: true, 
                    exclusive: false,           //队列是否是排他的
                    autoDelete: false,
                    arguments: null);
                  //3队列和交换机绑定
                   model.QueueBind(queue: queuename, 
                                exchange:  exchangeName,
                                routingKey: "", 
                                arguments: null);   //
            }
           
            //4发送消息
            while(true)
            {
                #region 处理输入

                                Console.WriteLine("输入要发送的消息,输入Q退出程序");
                                Console.Write("请输入订单编号：");
                                string orderNo= Console.ReadLine();
                                if (orderNo == "Q") break;
                                Console.Write("请输入姓名：");
                                string name = Console.ReadLine();
                                if (name == "Q") break;

                                Console.Write("请输入电话：");
                                string phone = Console.ReadLine();
                                if (phone == "Q") break;
                                Console.Write("请输入Email：");
                                string  email = Console.ReadLine();
                                if (email == "Q") break;
                                Console.Write("请输入微信号：");
                                string  wx = Console.ReadLine();
                                if (wx == "Q") break;
                                Console.Write("输入结束，开始发送Order消息===========");
                                Message.OrderMessage msg = new Message.OrderMessage();
                                msg.OrderId = orderNo;
                                msg.Phone = phone;
                                msg.Email = email;
                                msg.Wx = wx;
                                msg.UserName = name;
                #endregion

                string jsonMsg = Newtonsoft.Json.JsonConvert.SerializeObject(msg); //数据序列化为json 字符串
                byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(jsonMsg);

                IBasicProperties baseProperties= model.CreateBasicProperties();
                baseProperties.DeliveryMode = 2;  //1 非持久 2持久
                baseProperties.ContentType = "application/json"; //消息体协议
               // baseProperties.Expiration = "2000"; //消息失效时间,毫秒
                model.ConfirmSelect();      //启用发布者确认模式，确保消息投递到host
                model.BasicPublish(exchange: exchangeName, 
                    routingKey: "",
                    mandatory: false, 
                    basicProperties: baseProperties,
                    body: messageBodyBytes);
                bool confim= model.WaitForConfirms();  //同步
                Console.WriteLine("消息投递到HOST:" + confim);
            }

            Console.WriteLine("按任意键结束程序！");
            Console.ReadLine();
            //5.关闭连接
            model.Close();
            connection.Close();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

      
    }
}
