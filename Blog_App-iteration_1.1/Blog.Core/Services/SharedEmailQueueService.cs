using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text.Json;
using System.Runtime.Versioning;
using Blog.Core.Models;
using Blog.Core.Interfaces;
using Blog.Core.Constants;

namespace Blog.Core.Services
{


    [SupportedOSPlatform("windows")]
    public class SharedEmailQueueService : ISharedEmailQueueService
    {
        private readonly ConcurrentQueue<EmailQueueMessage> _emailQueue;
        private NamedPipeServerStream? _pipeServer;
        private readonly string _pipeName = EmailQueueConstants.DefaultPipeName;
        private readonly bool _isServer;

        public SharedEmailQueueService(bool isServer = true)
        {
            _emailQueue = new ConcurrentQueue<EmailQueueMessage>();
            _isServer = isServer;

            if (_isServer)
            {
                _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In, EmailQueueConstants.MaxPipeInstances, PipeTransmissionMode.Message);
                StartListening();
            }
        }

        private async void StartListening()
        {
            try
            {
                while (true)
                {
                    if (_pipeServer == null) return;
                    
                    await _pipeServer.WaitForConnectionAsync();
                    using (StreamReader reader = new StreamReader(_pipeServer))
                    {
                        string? messageJson = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(messageJson))
                        {
                            var message = JsonSerializer.Deserialize<EmailQueueMessage>(messageJson);
                            if (message != null)
                            {
                                _emailQueue.Enqueue(message);
                            }
                        }
                    }
                    _pipeServer.Disconnect();
                }
            }
            catch (Exception ex)
            {

                if (_pipeServer != null)
                {
                    _pipeServer.Dispose();
                    _pipeServer = null;
                }
                Thread.Sleep(EmailQueueConstants.RetryDelay); // Wait before restarting
                _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In, EmailQueueConstants.MaxPipeInstances, PipeTransmissionMode.Message);
                StartListening();
            }
        }

        public async void QueueEmail(string email, string subject, string htmlMessage)
        {
            var message = new EmailQueueMessage
            {
                Email = email,
                Subject = subject,
                HtmlMessage = htmlMessage,
                CreatedAt = DateTime.UtcNow
            };

            if (_isServer)
            {
                _emailQueue.Enqueue(message);
            }
            else
            {
                using (var pipeClient = new NamedPipeClientStream(EmailQueueConstants.LocalPipeServer, _pipeName, PipeDirection.Out))
                {
                    try
                    {
                        await pipeClient.ConnectAsync(15000); // 15 second timeout
                        using (StreamWriter writer = new StreamWriter(pipeClient))
                        {
                            string messageJson = JsonSerializer.Serialize(message);
                            await writer.WriteLineAsync(messageJson);
                            await writer.FlushAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error and implement retry logic
                        Console.WriteLine(string.Format(EmailQueueConstants.ConnectionFailureMessage, ex.Message));
                        // Retry after a short delay
                        Thread.Sleep(EmailQueueConstants.RetryDelay);
                        try
                        {
                            await pipeClient.ConnectAsync(15000); // 15 second timeout
                            using (StreamWriter writer = new StreamWriter(pipeClient))
                            {
                                string messageJson = JsonSerializer.Serialize(message);
                                await writer.WriteLineAsync(messageJson);
                                await writer.FlushAsync();
                            }
                        }
                        catch (Exception retryEx)
                        {
                            Console.WriteLine(string.Format(EmailQueueConstants.RetryFailureMessage, retryEx.Message));
                        }
                    }
                }
            }
        }

        public bool TryDequeueEmail(out EmailQueueMessage emailMessage)
        {
            emailMessage = new EmailQueueMessage
            {
                Email = string.Empty,
                Subject = string.Empty,
                HtmlMessage = string.Empty,
                CreatedAt = DateTime.UtcNow
            };
            return _emailQueue.TryDequeue(out emailMessage);
        }
    }
}