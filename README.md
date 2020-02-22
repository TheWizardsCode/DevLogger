DevLogger is a Unity Plugin that helps you keep a DevLog while working on your project. The goal is to make it as easy as possible to create records of key milestones and to tell the world about them without interupting the development flow.

# Setup

## Register the application on Twitter

In preparation...

  1. Create a developer account on Twitter http://dev.twitter.com
  2. Register DevLogger at http://dev.twitter.com/apps/new
  3. Get the Consumer Token and Secret for the app
  4. Generate an Access Token and Access Token Secret
  5. Window -> Wizards Code -> Dev Logger
  6. Enter the consumer key and secret as well as the Access token and secret
 
# Using DevLogger

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

## Capture a ScreenShot

Screenshots can be captured at any time in the editor.

  1. Setup the view that you want to capture in the game window
  2. Click the "Capture Screenshot" button

Captured images are displayed in the `Media Capture` section for use in DevLog
entries.

Screenshots are stored in the `DevLog` folder in the
root of your project.

## Capture Animated GIFs

As well as capturing still images (see above) you can capture Animated GIFs when in
play mode:

  1. Enter play mode
  2. Hit the "Capture Animated Screenshot" button to capture 10 seconds of gameplay

It can take a short while for the gif to be encoded, but when it is
ready you will find it in the `DevLog` folder in the root of your
project.


