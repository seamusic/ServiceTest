using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RocketMQSender;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("消息队列发送者服务启动中...");

        // 获取消息发送服务
        var messageSender = host.Services.GetRequiredService<MessageSenderService>();

        // 启动消息发送任务
        var cancellationToken = new CancellationTokenSource();
        var sendTask = messageSender.StartSendingMessagesAsync(cancellationToken.Token);

        // 等待用户输入退出
        Console.WriteLine("按任意键停止发送消息...");
        Console.ReadKey();

        cancellationToken.Cancel();
        await sendTask;

        logger.LogInformation("消息队列发送者服务已停止");
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<MessageSenderService>();
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                });
            });
}

public class MessageSenderService
{
    private readonly ILogger<MessageSenderService> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    public MessageSenderService(ILogger<MessageSenderService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartSendingMessagesAsync(CancellationToken cancellationToken)
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

            _logger.LogInformation("消息队列发送者已启动");

            var messageCount = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 创建消息
                    var messageBody = $"测试消息 #{++messageCount} - 时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    var body = Encoding.UTF8.GetBytes(messageBody);

                    // 设置消息属性
                    var properties = new BasicProperties();
                    properties.MessageId = Guid.NewGuid().ToString();
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                    // 发送消息
                    await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, true, basicProperties: properties, body: body);

                    _logger.LogInformation($"消息发送成功: MessageId={properties.MessageId}, Message={messageBody}");

                    // 等待1秒后发送下一条消息
                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "发送消息时发生错误");
                    await Task.Delay(5000, cancellationToken); // 错误后等待5秒再重试
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("消息发送任务被取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "消息队列发送者服务发生错误");
        }
        finally
        {
            await _channel?.CloseAsync();
            await _connection?.CloseAsync();
            await _channel.DisposeAsync();
            await _connection.DisposeAsync();
            _logger.LogInformation("消息队列发送者已停止");
        }
    }

    public async Task SendSingleMessage(string messageBody)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Channel未初始化");
        }

        var body = Encoding.UTF8.GetBytes(messageBody);
        var properties = new BasicProperties();
        properties.MessageId = Guid.NewGuid().ToString();

        await _channel.BasicPublishAsync(exchange: "", routingKey: "test-queue", true, basicProperties: properties, body: body);

        _logger.LogInformation($"单条消息发送成功: MessageId={properties.MessageId}");
    }
}