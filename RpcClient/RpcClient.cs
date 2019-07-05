using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//https://github.com/rabbitmq/rabbitmq-tutorials/blob/master/dotnet-visual-studio/6_RPCClient/RPCClient.cs
namespace RpcClient
{
   public class RpcClient
    {

        private const string QUEUE_NAME = "rpc_queue";

        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;

        private ConcurrentDictionary<string, string> resDic = new ConcurrentDictionary<string, string>();

        public RpcClient()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = "10.2.56.245";
            connectionFactory.Port = 5672; //默认端口
            connectionFactory.VirtualHost = "/";
            connectionFactory.UserName = "hw";
            connectionFactory.Password = "hw";
            connectionFactory.AutomaticRecoveryEnabled = true;
            connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);

            connection = connectionFactory.CreateConnection("rpcclient");
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);
            consumer.Received += (obj, es) =>
            {
                string res = Encoding.UTF8.GetString(es.Body);
                resDic.TryAdd(es.BasicProperties.CorrelationId, res);
            };

            channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

        }

        public string Call(string message)
        {
            var corrId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = corrId;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: props, body: messageBytes);
            string res = "";
            while (res=="")
            {
                if(resDic.ContainsKey(corrId))
                {
                    resDic.TryRemove(corrId, out res);
                }
            }
            return res;
        }

        public void Close()
        {
            connection.Close();
        }

    }
}
