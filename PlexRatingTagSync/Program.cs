using PlexRatingTagSync;
using System.CommandLine;
using System.Net.Http.Headers;
using System.Net.Http.Json;

// See https://aka.ms/new-console-template for more information

HttpClient client = new HttpClient();
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

var rootCommand = new RootCommand("Sync user rating in Plex to the file metadata tag for a music library.");
var serverBaseOption = new Option<string>(
        name: "--plex-host",
        description: "Plex server hostname and port."
);
serverBaseOption.IsRequired = true;
var plexTokenOption = new Option<string>(
        name: "--plex-token",
        description: "Your Plex token."
);
plexTokenOption.IsRequired = true;
var librarySectionOption = new Option<string>(
        name: "--library-section",
        description: "Section number for the music library."
);
librarySectionOption.IsRequired = true;
var modifiedDaysOption = new Option<int>(
        name: "--modified-days",
        description:  "Only sync files modified within this many days.",
        getDefaultValue: () => -1
);

var listLibrarySectionsCommand = new Command("list-library-sections", "List library sections by key.")
{
        serverBaseOption,
        plexTokenOption
};
rootCommand.AddCommand(listLibrarySectionsCommand);

listLibrarySectionsCommand.SetHandler(async (serverBase, plexToken) =>
{
    var options = new SyncOptions(
        serverBase,
        plexToken,
        String.Empty,
        0);

    await ListLibrarySectionsAsync(client, options);
},
serverBaseOption, plexTokenOption);

var syncToPlexCommand = new Command("sync-to-plex", "Sync ratings from files to Plex.") 
{
        serverBaseOption,
        plexTokenOption,
        librarySectionOption,
        modifiedDaysOption
};
rootCommand.AddCommand(syncToPlexCommand);

syncToPlexCommand.SetHandler(async (serverBase, plexToken, librarySection, modifiedDays) =>
{
    var options = new SyncOptions(
        serverBase,
        plexToken,
        librarySection,
        modifiedDays);

    await SyncAllRatingsAsync(client, options);
},
serverBaseOption, plexTokenOption, librarySectionOption, modifiedDaysOption);

return await rootCommand.InvokeAsync(args);

static async Task ListLibrarySectionsAsync(HttpClient client, SyncOptions options)
{
    PlexRes? res;
    string url = String.Empty;

    try
    {
        url = $"{options.ServerBase}/library/sections?X-Plex-Token={options.PlexToken}";
        res = await client.GetFromJsonAsync<PlexRes>(url);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to library sections");
        Console.WriteLine($"Request URL: {url}");
        Console.WriteLine(ex.StackTrace);
        return;
    }

    var dirs = res!.MediaContainer.Directory.OrderBy((d) => { return d.Key; }).ToArray();
    if (dirs != null)
    {
        Console.WriteLine($"{"Library Section",-15}    Title");
        for (var i = 0; i < dirs.Length; i++)
        {
            Console.WriteLine($"{dirs[i].Key.PadRight(15, '.')}....{dirs[i].Title}");
        }
    }
}

static async Task SyncAllRatingsAsync(HttpClient client, SyncOptions options)
{
    PlexRes? res;
    string url = String.Empty;

    if (options.SyncThreshold != null)
    {
        Console.WriteLine($"Only sync files modified after {options.SyncThreshold}");
    }

    try
    {
        url = $"{options.ServerBase}/library/sections/{options.LibrarySection}/albums?X-Plex-Token={options.PlexToken}";
        res = await client.GetFromJsonAsync<PlexRes>(url);
    }
    catch (Exception ex) {
        Console.WriteLine($"Failed to list library albums");
        Console.WriteLine($"Request URL: {url}");
        Console.WriteLine(ex.StackTrace);
        return;
    }

    var albums = res!.MediaContainer.Metadata;
    if (albums != null)
    {
        var total = albums.Length.ToString().PadLeft(6, '0');
        for (var i = 0; i < albums.Length; i++)
        {
            var pos = (i + 1).ToString().PadLeft(6, '0');
            Console.WriteLine($"[{pos}/{total}] Rating Album: {albums[i].Key} - {albums[i].Title}");
            await SyncAlbumRatingsAsync(client, options, albums[i].Key);
        }
    }
}

static async Task SyncAlbumRatingsAsync(HttpClient client, SyncOptions options, string key)
{
    PlexRes? res;
    string url = String.Empty;

    try
    {
        url = $"{options.ServerBase}{key}?X-Plex-Token={options.PlexToken}";
        res = await client.GetFromJsonAsync<PlexRes>(url);
        if (res == null)
        {
            throw new Exception("Plex request failed");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to get metadata for: {key}");
        Console.WriteLine($"Request URL: {url}");
        Console.WriteLine(ex.StackTrace);
        return;
    }

    foreach (var metadata in res.MediaContainer.Metadata)
    {
        if (metadata == null)
        {
            continue;
        }

        // Get the first file associated with the track.
        string path = metadata.FirstMedia?.FirstFile ?? String.Empty;
        if (path == String.Empty)
        {
            Console.WriteLine($"No media for: {metadata.Key}");
            continue;
        }

        try
        {
            // Compare file modification time to threshold date.
            if ((options.SyncThreshold != null) && (File.GetLastWriteTime(path) < options.SyncThreshold))
            {
                Console.WriteLine($"Skip rating: {path}");
                continue;
            }

            try
            {
                // Parse tag and rate file in Plex.
                var model = new ATL.Track(path);
                if (model.Popularity != null)
                {
                    Console.WriteLine($"Rating Track: {metadata.Key} - {metadata.Title} - {model.Popularity}");
                    await SetRatingAsync(client, options, metadata.RatingKey, model.Popularity * 10.0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read tag from: {path}");
                Console.WriteLine(ex.StackTrace);
            }
        } 
        catch (FileNotFoundException)
        {
            Console.WriteLine($"File does not exist: {path} - try emptying trash in Plex.");            
        }
    }
}

static async Task SetRatingAsync(HttpClient client, SyncOptions options, string key, double? rating)
{
    string url = String.Empty;

    try
    {
        url = $"{options.ServerBase}/:/rate?X-Plex-Token={options.PlexToken}&key={key}&identifier=com.plexapp.plugins.library&rating={rating ??= -1}";
        await client.PutAsync(url, null);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to set rating for: {key}");
        Console.WriteLine($"Request URL: {url}");
        Console.WriteLine(ex.StackTrace);
        return;
    }
}