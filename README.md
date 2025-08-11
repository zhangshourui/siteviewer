# Site Viewer
A web application that displays files and directories on a server, allowing users to download content or stream videos.

# Project Structure
## siteviewer.spa
React-based single-page application with client-side routing, created by react-router.
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

### Building for Production

Create a production build:

```bash
npm run build
```
  
## SiteViewer.WWW (Server)
ASP.NET Core 9 server providing APIs and file services.

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
Settings are in appsettings-{environment}.json:
#### Configuration
| Key                        | Type     | Default Value | Description                                                                                                                                                                    |
|----------------------------|----------|---------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Host`                     | string   | -             | The api site host                                                                                                                                                              |
| `ResourceRoot`             | string   | -             | The resource root user will view                                                                                                                                               |
| `ResourceCache`            | string   | -             | The cache directory. Temporarily generated files will be placed here, such as video streams                                                                                    |
| `AccessControlAllowOrigin` | string[] | -             | A list of hosts allowed for cross-origin access, and the host of siteviewer.spa should be included.                                                                            |
| `Ffmpeg`                   | string   | ffmpeg        | Specify the path to the FFmpeg.exe file. It is used to convert videos into streamable formats for online playback. If not configured, video playback will not be available.    |

### Dependences
- Visual Studio Users: Install [Web Compiler](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.WebCompiler) extension for SCSS/LESS compilation.

# Features
- File and directory browsing
- File download capability
- Video streaming (requires FFmpeg configuration)
- Cross-origin resource sharing (CORS) support
