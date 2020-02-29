A DevLog is an important communication device. It ensures that team members are aware of the work
you have done, and have a record of how you did it. It can also be used for marketing purposes, 
keeping everyone informed of what you are working on and the progress made.

DevLogger is a Unity Plugin that helps you keep a DevLog while working on your project. The goal is to make it as easy as possible to create records of key milestones and to tell the world about them without interupting the development flow.

# Features

  * Capture in-game screenshots
  * Capture in-game animated GIFs
  * Capture in-editor windows image (scene view, hierarchy window etc.)
  * Post timed notes, with or without images, to a markdown DevLog
  * Post update, with or without images, to Twitter
  * Open Source contributions welcome - lets be more productive together

# Setup

## Register the application on Twitter

In preparation...

  1. Create a developer account on Twitter http://dev.twitter.com
  2. Register DevLogger at http://dev.twitter.com/apps/new
  3. Get the Consumer Token and Secret for the app
  4. Generate an Access Token and Access Token Secret
  5. Tools -> Wizards Code -> Dev Logger
  6. Enter the consumer key and secret as well as the Access token and secret
 
# Using DevLogger

## Capture an in-game ScreenShots and Animated GIFs

When in play mode you can capture screenshots and animated GIFs right from within
the editor.

  1. Setup the view that you want to capture in the game window
  2. Click the "Game View" or "Animated GIF" button

Captured images are displayed in the `Media Capture` section for use in DevLog and Twitter
entries.

Screenshots are stored in the `DevLog` folder in the root of your project.

## Capture In-Editor Screenshots

When not on play mode you can capture shots of editor windows and then use those images in
DevLog entries.

  1. Simply hit one of the buttons in the "Media Capture"
## Record a Log Entry

Log entries are stored in files in the `DevLog` folder in the root of your project.
In summary:

  1. Enter your log entry text into the field. 
  2. Click "Tweet with text only" or "Tweet with text and image"
  3. If tweeting with an image select the desired image in the file selection dialog

To add an entry to the DevLog type a short description into the Log Entry text box.
DevLogs are supposed to be short at this point. The goal is to record a quick status
update so you know what you did and when. Click either the `DevLog (no tweet) with 
text only` or the `DevLog (no Tweet) with selected image and text` button to record
the DevLog in the file.

Note that, if your log entry is short enough, you can also post a DevLog as a Tweet.
Doing this (see below) will automatically record the entry in your DevLog file as
well as send it to Twitter.

If you choose the `with image` option then the image selected in the `Media Capture`
section is linked to the entry. See below for details on how you capture screenshots
and animated GIFs.

## Send a Tweet

As noted above you can optionally send a DevLog entry to Twitter as well as record it
in your local DevLog file. When sending the entry to twitter appropriate hashtags will
also be added. As with DevLog entries you can optionally include a still image or an 
animated GIF.

  1. Enter your log entry text into the field. 
  2. Click "Tweet with text only" or "Tweet with text and image"
  3. If tweeting with an image select the desired image in the file selection dialog

The text entered must be shorter than the 140 chars minus the length of 
the selected hastags. If it is too long the tweet buttons will not be 
available to you.


# Known Issues

Here are the known issues we are tracking and intend to fix (patches welcome to speed things up).
If you are facing an issue that is not in this list but it blocking work please open a GitHub issue
and report it - preferably with a patch to fix it, but don't worry, just telling us it is important
to you is helpful.

  * Media Capture previews are incomplete when captured in the editor, though they will complete when switching to/from play mode
  * Animated GIFs have incorrect coloring

# TODO

This is a loose collection of things that are on the immediate roadmap. Nothing here is 
promised. If you really want one of these items please implement it and issue a pull
request, same goes for other features not listed here.

  * Animated GIFs
	  - Takes a long time to encode make it impossible to press a capture button until finished
  * Tweeting
	  - Make the hashtags used configurable on a per tweet basis
	  - Automatically append a link to any tweet


