#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Text.Json;
using System.Text.Json.Serialization;
using Origam.Architect.Server.Configuration;
using Origam.Architect.Server.Models;
using Origam.Extensions;

namespace Origam.Architect.Server.Services;

public class ChatHistoryService
{
    private const string AiDirectoryName = "ai";
    private const string ChatsDirectoryName = "chats";
    private const string ImagesDirectoryName = "images";
    private const string ThreadFileName = "thread.json";
    private const int MaxImageBytes = 10 * 1024 * 1024;

    private static readonly Dictionary<string, string> ExtensionByMimeType = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/gif"] = ".gif",
        ["image/webp"] = ".webp",
    };

    private static readonly Dictionary<string, string> MimeTypeByExtension = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
    };

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly ILogger<ChatHistoryService> logger;
    private readonly object writeLock = new();
    private readonly string storageDirectory;

    public ChatHistoryService(ILogger<ChatHistoryService> logger, IConfiguration configuration)
    {
        this.logger = logger;
        storageDirectory = ResolveStorageDirectory(configuration);
        Directory.CreateDirectory(storageDirectory);
    }

    public List<ChatThreadModel> LoadAll()
    {
        var threads = new List<ChatThreadModel>();
        foreach (string directory in Directory.GetDirectories(storageDirectory))
        {
            ChatThreadModel thread = ReadThread(Path.Combine(directory, ThreadFileName));
            if (thread != null)
            {
                threads.Add(thread);
            }
        }
        return threads.OrderByDescending(thread => thread.CreatedAt).ToList();
    }

    public void Save(ChatThreadModel thread)
    {
        string json = JsonSerializer.Serialize(thread, SerializerOptions);
        lock (writeLock)
        {
            string directory =
                FindThreadDirectory(thread.Id)
                ?? Path.Combine(storageDirectory, BuildDirectoryName(thread));
            Directory.CreateDirectory(directory);
            WriteAllTextAtomically(Path.Combine(directory, ThreadFileName), json);
        }
    }

    public bool Delete(Guid threadId)
    {
        lock (writeLock)
        {
            string directory = FindThreadDirectory(threadId);
            if (directory == null)
            {
                return false;
            }
            Directory.Delete(directory, recursive: true);
            return true;
        }
    }

    public bool SaveImage(ChatImageRequest request)
    {
        if (
            !ExtensionByMimeType.TryGetValue(request.MimeType ?? string.Empty, out string extension)
        )
        {
            throw new UserOrigamException(Strings.ChatImageTypeNotSupported);
        }
        byte[] content = DecodeImage(request.Data);

        lock (writeLock)
        {
            string threadDirectory = FindThreadDirectory(request.ThreadId);
            if (threadDirectory == null)
            {
                return false;
            }
            string imagesDirectory = Path.Combine(threadDirectory, ImagesDirectoryName);
            Directory.CreateDirectory(imagesDirectory);
            string file = Path.Combine(imagesDirectory, $"{request.ImageId:D}{extension}");
            if (!File.Exists(file))
            {
                string temporaryFile = file + ".tmp";
                File.WriteAllBytes(temporaryFile, content);
                File.Move(temporaryFile, file, overwrite: true);
            }
            return true;
        }
    }

    public ChatImageContent ReadImage(Guid threadId, Guid imageId)
    {
        string threadDirectory = FindThreadDirectory(threadId);
        if (threadDirectory == null)
        {
            return null;
        }
        string imagesDirectory = Path.Combine(threadDirectory, ImagesDirectoryName);
        if (!Directory.Exists(imagesDirectory))
        {
            return null;
        }
        foreach (string file in Directory.GetFiles(imagesDirectory, $"{imageId:D}.*"))
        {
            if (MimeTypeByExtension.TryGetValue(Path.GetExtension(file), out string mimeType))
            {
                return new ChatImageContent(File.ReadAllBytes(file), mimeType);
            }
        }
        return null;
    }

    private static byte[] DecodeImage(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            throw new UserOrigamException(Strings.ChatImageInvalid);
        }
        long minimumDecodedLength = ((long)data.Length / 4 * 3) - 2;
        if (minimumDecodedLength > MaxImageBytes)
        {
            throw new UserOrigamException(Strings.ChatImageTooLarge);
        }
        byte[] content;
        try
        {
            content = Convert.FromBase64String(data);
        }
        catch (FormatException)
        {
            throw new UserOrigamException(Strings.ChatImageInvalid);
        }
        if (content.Length > MaxImageBytes)
        {
            throw new UserOrigamException(Strings.ChatImageTooLarge);
        }
        return content;
    }

    private ChatThreadModel ReadThread(string file)
    {
        if (!File.Exists(file))
        {
            return null;
        }
        try
        {
            var thread = JsonSerializer.Deserialize<ChatThreadModel>(
                File.ReadAllText(file),
                SerializerOptions
            );
            return thread?.Id == Guid.Empty ? null : thread;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, message: "Cannot read chat history file {File}", file);
            return null;
        }
    }

    private string FindThreadDirectory(Guid threadId)
    {
        return Directory.GetDirectories(storageDirectory, $"*{threadId:D}").FirstOrDefault();
    }

    private static string BuildDirectoryName(ChatThreadModel thread)
    {
        return $"{thread.CreatedAt.ToUniversalTime():yyyyMMdd-HHmmss}-{thread.Id:D}";
    }

    private static void WriteAllTextAtomically(string file, string content)
    {
        string temporaryFile = file + ".tmp";
        File.WriteAllText(temporaryFile, content);
        File.Move(temporaryFile, file, overwrite: true);
    }

    private static string ResolveStorageDirectory(IConfiguration configuration)
    {
        string configuredPath = configuration.GetValue<string>(key: "ChatHistory:Path");
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.GetFullPath(configuredPath);
        }

        string clientApplicationPath = configuration
            .GetSectionOrThrow("SpaConfig")
            .Get<SpaConfig>()
            .PathToClientApplication;
        if (string.IsNullOrWhiteSpace(clientApplicationPath))
        {
            throw new Exception(Strings.ChatHistoryLocationUnknown);
        }
        return Path.GetFullPath(
            Path.Combine(clientApplicationPath, AiDirectoryName, ChatsDirectoryName)
        );
    }
}
