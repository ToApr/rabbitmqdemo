using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderWorker
{
   public class MyBasicConsumer: DefaultBasicConsumer
    {

        public MyBasicConsumer(IModel model):base(model)
        {
           
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            Console.WriteLine("consumerTag:" + consumerTag);  //消费者标签
            Console.WriteLine("deliveryTag:" + deliveryTag);  //投递标签
            Console.WriteLine("redelivered:" + redelivered);  //是否是再次投递的消息
            Console.WriteLine("exchange:" + exchange);
            Console.WriteLine("routingKey:" + routingKey);
           
            Console.WriteLine("body:" + System.Text.Encoding.UTF8.GetString(body));

            //手工签收
            string queueName = System.Configuration.ConfigurationManager.AppSettings["queuename"];

            try
            {
            
                Console.WriteLine("模拟执行任务：发送"+ queueName);
            
                Model.BasicAck(deliveryTag, false);        //一定不要忘记手工签收
            }
            catch (Exception ex)              //异常的时候，重回队列，重新派发
            {

                Model.BasicNack(deliveryTag, false,true);
            }

            //base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
        }
    }
}
