# FromSpotifyToYoutube
Convert your Spotify playlist to a Youtube playlist using shareable Spotify playlist url.


## Configuration
### Spotify
- Replace SPOTIFY_CLIENT_ID, SPOTIFY_CLIENT_SECRET with your spotify developer account credentials.
### MongoDB
- MongoDB is used for storing metadata, create the database with the name in appsettings.json and add its connection string in place of MONGODB_CONNECTION_STRING

### Youtube
- In your google developer settings, enable youtube data API and setup OAuth2 web application credentials. Replace the client id and client secret (GOOGLE_YOUTUBE_OAUTH2_CLIENT_ID and GOOGLE_YOUTUBE_OAUTH2_CLIENT_SECRET)


## How to run
- Install dotnet sdk (.NET 6)
- cd to the root directory and run dotnet restore.
- Run dotnet build to build the solution.
- Run dotnet run to run the application.

## TODO:
- [ ] Add tests
- [ ] Improve exception handling
- [ ] Implement retry mechanisms for requests
- [ ] Extend code to a Web API or application
