# Site Viewer
The web project can show the files &amp; directories on the server, and user can download it, or play the videos.

# Projects
## siteviewer.spa
The client site created by react-router.
### Getting Started
Install the dependencies:

```bash
npm install
```

### Development

Start the development server with HMR:

```bash
npm run dev
```

Your application will be available at `http://localhost:5173`.

## Building for Production

Create a production build:

```bash
npm run build
```
  
## SiteViewer.WWW
The server-side site, which can provide apis, created by dotnet 9.

### Getting Started

Development:
```bash
dotnet run SiteViewer.WWW
```
Build & publish 
```bash
cd SiteViewer.WWW
# build
dotnet build .
# publish
dotnet publish . --output ../publish -c release
```
### Configuration
All configuration is in appsettings-{environment}.json.

- Host: The api site host。
- ResourceRoot：The resource root user will view.
- ResourceCache: The cache directory. Temporarily generated files will be placed here, such as video streams.
- AccessControlAllowOrigin：A list of hosts allowed for cross-origin access, and the host of siteviewer.spa should be included.
- Ffmpeg: Specify the path to the FFmpeg.exe file. It is used to convert videos into streamable formats for online playback. If not configured, video playback will not be available.



