
A DevLog is an important communication device. It ensures that team members are aware of the work
you have done, and have a record of how you did it. It can also be used for marketing purposes, 
keeping everyone informed of what you are working on and the progress made.

DevLogger is a Unity Plugin that helps you keep a DevLog while working on your project. The goal is to make it as easy as possible to create records of key milestones and to tell the world about them without interupting the development flow.

# Features

See [below](#using-devlogger) for usage guide

  * Capture in-game screenshots
  * Capture in-game animated GIFs
  * Capture in-editor windows image (scene view, hierarchy window etc.)
  * Post timed notes, with or without images, to a markdown DevLog
  * Manage hashtags
  * Create Dev Log entries from Git commits
  * Post updates, with or without images, to Twitter
  * Open Source contributions welcome - lets be more productive together

# Installation Of Latest Release

  1. `Window -> Package Manager`
  2. Click the '+" in the top left
  3. Select 'Add package from Git URL'
  4. Paste in `https://github.com/TheWizardsCode/DevLogger.git#release/stable`
  
# Installation Of Development Code

  1. Clone the repo into your preferred location
  2. `Window -> Package Manager`
  3. Click the '+" in the top left
  4. Select 'Add package from disk ...'
  5. Point to the directory containing your development

# Using DevLogger

When you first install you need to setup the storage databases:

  1. Select a location to store your captured images in "Captures Save Folder". You can, optionally, have your captures saved by project and scene subfolders.
  2. Create a DevLog Scriptable Object to organize your DevLog files. The easiest way to do this is simply click the "Create" button on the setup screen. The Scriptable Object will be created in the root of your Assets folder. You can move it if you want to.
  3. Create a Screen Capture Scriptable Object to organize your Screen Captures. The easiest way to do this is simply click the "Create" button on the setup screen. The Scriptable Object will be created in the root of your Assets folder. You can move it if you want to.
  
Once these steps are completed the Entry UI will be displayed.

## Entry Tab

The entry tab is where you will spend most of your time. It consists of the following sections:

### Log Entry

This has the short and long text for your log entry. Bear in mind that the short entry will be used when posting to social media. The detail entry will be used in the Dev Log only.

### Meta Data

The meta data section defines Hashtags, URLs and Git commit hashes that should be used alongside your descriptive text. Hashtags and URLs will be used in social media postings as well, so be aware that they will consume your character limit.

### Posting

This section are where actions for posting to your dev log will appear when a valid entry is available.

### Data

Here you can access the DevLog in MarkDown format.

### Media

This section shows thumbnails of the media files you have collected. Each thumbnail can be selected for inclusion in a develog or social media post. You can also view the images full size or open the media storage folder from here.

### Capture

When in Edit mode this section has a number of buttons enabling you to capture various editor windows. The captures will appear in the Media section (above) when complete.

When in Run mode this section allows you to capture stills and animated gifs. These captures will also appear in the Media section. 

Animated Gifs will capture a defined number of seconds before the capture button was pressed. That is, pressing the button does not mark the start of the capture, rather it marks the end of the capture. The quality and duration of the capture is configurable in the editor window.

Note it takes a short while for animated gifs to be processed, the will not appear in the media section until processing is complete.

### Twitter

To use Twitter you need to setup keys to allow the application to access your twitter account. If this is not setup yet the UI will present the fields to enter these keys. See below for instructions on how to create these tokens.

Once configured this section will allow you to post the short description with hashtags (see above) and up to 4 images or 1 animated gif to Twitter. Note animated gifs have a limit of 15Mb so keep them small. We'd love someone to integrate a service such as Giphy.com :-)

## Dev Log Tab

When a DevLog entry is made it will appear in this tab along with the meta data and images posted with it. You can edit the information here. You can also change the order the devlogs will appear in the final output.

## Git Tab

The Git tab enables you to view the git logs for this project. This is only tested on Windows, we welcome reports and patches for other platforms.

You can click a button on the log entries to copy the data over to the DevLog entry fields. 

# Twitter Setup

If you want to tweet from within DevLogger you will need to setup authentication
for Twitter but following these steps:

  1. Create a developer account on Twitter http://dev.twitter.com
  2. Register DevLogger at http://dev.twitter.com/apps/new
  3. Get the Consumer Token and Secret for the app
  4. Generate an Access Token and Access Token Secret
  5. In the DevLogger window expand the Twitter section
  6. Enter the consumer key and secret as well as the Access token and secret
  7. The twitter section will change to the twitter controls
  

# Release Process

We use [PackageTools](https://github.com/3dtbd/unity-package-tools) to create our releases. To build a release:

  0. Alongside your working repository checkout the `release/stable` branch of this repo
  1. Update (at least) the version number in the `PackageManifestConfig` in the root of the `Assets` folder
  2. Click `Generate VersionConstants.cs` in the inspector
  3. Commit the new constants file to Git
  4. Click `Export Package Source`
  5. Commit and push the changes in your release project to GitHub
