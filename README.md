# Plex Rating Tag Sync

Set the Plex user rating for all tracks in a music library to match the metadata tag (ID3v2, etc) rating.

You need to obtain your Plex token and the section umber of the library to update.

See: https://support.plex.tv/articles/204059436-finding-an-authentication-token-x-plex-token/

## Sync from Metadata Tag to Plex User Rating

This mode will read the popularity field from the metadata tag and write it into Plex's user rating field.

```sh
Usage:
  PlexRatingTagSync sync-to-plex [options]

Options:
  --plex-host <plex-host> (REQUIRED)              Plex server hostname and port.
  --plex-token <plex-token> (REQUIRED)            Your Plex token.
  --library-section <library-section> (REQUIRED)  Section number for the music library.
  --modified-days <modified-days>                 Only sync files modified within this many days. [default: -1]
  -?, -h, --help                                  Show help and usage information

Example:
  sync-to-plex --plex-host 10.11.1.201:32400 --plex-token [REDACTED] --library-section 5 --modified-days 1
```

## Sync from Plex User Rating to Metadata Tag

This mode will read Plex's user rating field and write it into the popularity field from the metadata tag.

Not implemented yet.