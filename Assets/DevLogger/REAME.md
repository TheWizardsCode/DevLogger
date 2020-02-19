DevLogger is a Unity Plugin that helps you keep a DevLog while working on your project. The goal is to make it as easy as possible to create records of key milestones and to tell the world about them without interupting the development flow.

# Setup

## Register the application on Twitter

In preparation...

  1. Create a developer account on Twitter
  2. Register DevLogger at http://dev.twitter.com/apps/new
  3. Get the Consumer Token and Secret for the app
  4. Generate an Access Token and Access Token Secret
  5. Window -> Wizards Code -> Dev Logger
  6. Enter the consumer key and secret as well as the Access token and secret
 
# Using DevLogger

## Record a Log Entry

Log entries are sent to Twitter with selected hashtags. Optionally
they can include a still image or an animated GIF.

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

Screenshots are stored in the `Screenshots` folder in the
root of your project.

## Capture Animated GIFs

When in play mode animated GIFs can be captured.

  1. Enter play mode
  2. Hit the "Capture Animated Screenshot" button

It can take a short while for the gif to be encoded, but when it is
ready you will find it in the `Screenshots` folder in the root of your
project.


