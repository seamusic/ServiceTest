using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RocketMQSubscriber;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("消息队列订阅者服务启动中...");

        await host.RunAsync();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<MessageSubscriberService>();
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                });
            });
}

public class MessageSubscriberService : BackgroundService
{
    private readonly ILogger<MessageSubscriberService> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    public MessageSubscriberService(ILogger<MessageSubscriberService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // 配置RabbitMQ连接参数
            var factory = new ConnectionFactory()
            {
                HostName = "172.18.0.12",
                Port = 10911,
                UserName = "guest",
                Password = "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // 声明队列
            var queueName = "test-queue";
            await _channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // 创建消费者
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation($"收到消息: RoutingKey={ea.RoutingKey}, MessageId={ea.BasicProperties.MessageId}");
                    _logger.LogInformation($"消息内容: {message}");

                    // 处理消息逻辑
                    ProcessMessage(message);

                    // 确认消息处理完成
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理消息时发生错误");
                    // 拒绝消息并重新入队
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            // 开始消费消息
            await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("消息队列订阅者已启动，等待消息...");

            // 等待取消信号
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("服务正在停止...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "消息队列订阅者服务发生错误");
        }
    }

    private void ProcessMessage(string message)
    {
        // 这里实现具体的消息处理逻辑
        _logger.LogInformation($"处理消息: {message}");

        // 模拟处理时间
        Thread.Sleep(100);

        _logger.LogInformation("消息处理完成");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _channel?.CloseAsync();
        await _connection?.CloseAsync();
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
        _logger.LogInformation("消息队列订阅者已停止");

        await base.StopAsync(cancellationToken);
    }
}