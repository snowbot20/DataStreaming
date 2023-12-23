using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StreamingApp.Models;

namespace StreamingApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private const string DataProviderMethodName = "GetDataChunk";
    private const int ChunkSize = 10240; // 10 KB - Service can return only 10KB data at a time.
    private const string SessionKey = "VideoStreamPosition";    

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet("stream")]
    public IActionResult StreamVideo()
    {
        // TODO - Get the API to support audio player, fast forward

        // Get the current position from the session
        var currentPosition = HttpContext.Session.GetInt32(SessionKey) ?? 0;

        // Call the data provider method to get the next chunk
        // TODO: can expect only 10KB data, write logic to collect data based on 
        // request range headers coming from browser
        var dataChunk = GetDataChunk(currentPosition, ChunkSize);

        // Set response headers for partial content (range request)
        Response.Headers.Add("Content-Range", $"bytes {currentPosition}-{currentPosition + dataChunk.Length - 1}/*");
        Response.Headers.Add("Content-Length", dataChunk.Length.ToString());

        // Set the response status code to 206 Partial Content
        Response.StatusCode = 206;

        // Set the content type based on the data type
        Response.Headers.Add("Content-Type", "audio/mp4");

        // Update session variable with the current position
        HttpContext.Session.SetInt32(SessionKey, currentPosition + dataChunk.Length);

        // Return the data chunk to the client
        return File(dataChunk, "audio/mp4");
    }

    private byte[] GetDataChunk(int startPosition, int chunkSize)
    {
        // Replace this with the actual logic to fetch data from your service or another source
        // For demonstration purposes, read a file from the server

        // The service has a limitation of sending only 10KB at a time
        // so this is mock created for the same

        var filePath = "D:/Pics/Videos/20141005_111414.mp4";

        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            fileStream.Seek(startPosition, SeekOrigin.Begin);
            var buffer = new byte[chunkSize];
            var bytesRead = fileStream.Read(buffer, 0, chunkSize);

            // Trim the buffer to the actual number of bytes read
            return bytesRead < chunkSize ? buffer.Take(bytesRead).ToArray() : buffer;
        }
    }
}
