using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

if (args == null || args.Length == 0)
{
    throw new ArgumentException("First parameter must be the webhook token, second one a link to the image, other ones are the message to send\n" +
        "Image link and text are optional but you must provide at least one of them.", nameof(args));
}
var url = args[0];

List<Embed> embeds = new();
MultipartFormDataContent form = new();

// Loop through all files and add them if they are valid local files
int i = 1;
for (; i < args.Length; i++)
{
    if (File.Exists(args[i]))
    {
        FileInfo fi = new(args[i]);
        embeds.Add(new Embed
        {
            Url = "https://twitter.com", // https://github.com/discord/discord-api-docs/issues/1643#issuecomment-652557246
            Image = new Image
            {
                Url = "attachment://" + "file" + i + fi.Extension
            }
        });
        form.Add(new StreamContent(File.OpenRead(args[i])), "file" + i, "file" + i + fi.Extension);
    }
    else
    {
        break;
    }
}

// Add content
if (i < args.Length)
{
    if (embeds.Count == 0)
    {
        embeds.Add(new());
    }
    embeds[0].Description = string.Join(" ", args.Skip(i));
    embeds[0].Color = 65280; // Green
}

form.Add(new StringContent(JsonSerializer.Serialize(new PayloadRequest { Embeds = embeds.ToArray() })), "payload_json");

// Http request to webhook
using var http = new HttpClient();
var resp = await http.PostAsync(url, form);
resp.EnsureSuccessStatusCode();

class Image
{
    [JsonPropertyName("url")]
    public string Url { init; get; }
}

class Embed
{
    [JsonPropertyName("description")]
    public string Description { set; get; }
    [JsonPropertyName("image")]
    public Image Image { init; get; }
    [JsonPropertyName("url")]
    public string Url { init; get; }
    [JsonPropertyName("color")]
    public int Color { set; get; }
}

class PayloadRequest
{
    [JsonPropertyName("embeds")]
    public Embed[] Embeds { init; get; }
}