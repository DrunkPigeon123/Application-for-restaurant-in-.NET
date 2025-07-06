using Microsoft.EntityFrameworkCore;
  using restaurant.Data;
  using System.Net.WebSockets;
  using System.Text;
  using System.Collections.Concurrent;

  var builder = WebApplication.CreateBuilder(args);
  builder.WebHost.UseUrls("http://localhost:5000", "http://0.0.0.0:5000");

  builder.Services.AddControllers();
  builder.Services.AddDbContext<ApplicationDBContext>(options =>
  {
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
  });
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
  });

  var app = builder.Build();
  app.UseCors("AllowAll");
  app.UseWebSockets();
  app.UseDefaultFiles();
  app.UseStaticFiles();
  app.MapGet("/", () => Results.File("index.html", "text/html"));
  app.MapGet("/przepisy", () =>
  {
      var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "przepisy.html");
      Console.WriteLine($"Checking for przepisy.html at: {path}");
      if (File.Exists(path))
      {
          Console.WriteLine("przepisy.html found");
          return Results.File("przepisy.html", "text/html");
      }
      Console.WriteLine("przepisy.html not found");
      return Results.NotFound("przepisy.html not found");
  });

  var webSocketClients = new ConcurrentBag<WebSocket>();
  app.Map("/ws", async context =>
  {
      if (context.WebSockets.IsWebSocketRequest)
      {
          using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
          webSocketClients.Add(webSocket);
          Console.WriteLine($"WebSocket client connected. Total clients: {webSocketClients.Count}");
          var buffer = new byte[1024 * 4];
          while (webSocket.State == WebSocketState.Open)
          {
              var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
              if (result.MessageType == WebSocketMessageType.Text)
              {
                  var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                  Console.WriteLine($"Received: {message}");
                  var messageBytes = Encoding.UTF8.GetBytes(message);
                  foreach (var client in webSocketClients.ToArray())
                  {
                      if (client.State == WebSocketState.Open)
                      {
                          Console.WriteLine($"Sending to client: {client.State}");
                          await client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                      }
                      else
                      {
                          webSocketClients.TryTake(out _);
                      }
                  }
              }
              else if (result.MessageType == WebSocketMessageType.Close)
              {
                  webSocketClients.TryTake(out _);
                  Console.WriteLine($"WebSocket client disconnected. Total clients: {webSocketClients.Count}");
                  await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
              }
          }
      }
      else
      {
          context.Response.StatusCode = 400;
      }
  });

  app.MapControllers();
  app.Run();