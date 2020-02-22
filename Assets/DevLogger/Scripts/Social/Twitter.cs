using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Social
{
    public class Twitter
    {
        public const string EDITOR_PREFS_TWITTER_USER_ID = "TwitterUserID";
        public const string EDITOR_PREFS_TWITTER_USER_SCREEN_NAME = "TwitterUserScreenName";
        public const string EDITOR_PREFS_TWITTER_API_KEY = "TwitterAPIKey";
        public const string EDITOR_PREFS_TWITTER_API_SECRET = "TwitterAPISecret";
        public const string EDITOR_PREFS_TWITTER_ACCESS_TOKEN = "TwitterAccessToken";
        public const string EDITOR_PREFS_TWITTER_ACCESS_SECRET = "TwitterAccessSecret";

        private const string PostTweetURL = "https://api.twitter.com/1.1/statuses/update.json";
        private const string UploadMediaURL = "https://upload.twitter.com/1.1/media/upload.json";

        /// <summary>
        /// Test to see if the correvt API key and Access tokens have been provided.
        /// There are stored in EditorPrefs with the following keys:
        /// Twitter.EDITOR_PREFS_TWITTER_API_KEY
        /// Twitter.EDITOR_PREFS_TWITTER_API_SECRET
        /// Twitter.EDITOR_PREFS_TWITTER_ACCESS_TOKEN
        /// Twitter.EDITOR_PREFS_TWITTER_ACCESS_SECRET  
        /// </summary>
        /// /// <returns>True if correct values have been supplied.</returns>
        public static bool IsAuthenticated
        {
            get
            {
                if (string.IsNullOrEmpty(EditorPrefs.GetString(EDITOR_PREFS_TWITTER_API_KEY)))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(EditorPrefs.GetString(EDITOR_PREFS_TWITTER_API_SECRET)))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(EditorPrefs.GetString(EDITOR_PREFS_TWITTER_ACCESS_TOKEN)))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(EditorPrefs.GetString(EDITOR_PREFS_TWITTER_ACCESS_SECRET)))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Publish a text only tweet. See the console for error details.
        /// </summary>
        /// <param name="status">The text of the tweet.</param>
        /// <returns>True if succesfully published.</returns>
        public static bool PublishTweet(string status, out string response)
        {
            if (string.IsNullOrEmpty(status) || status.Length > 140)
            {
                response = string.Format("Text of tweet too long or too short at {0} chars.", status.Length);
                Debug.LogError(response);

                return false;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("status", status);

            WWWForm form = new WWWForm();
            form.AddField("status", status);

            Hashtable headers = GetHeaders(PostTweetURL, parameters);
            
            return ApiRequest(PostTweetURL, form, headers, out response);
        }

        public static bool PublishTweetWithMedia(string status, string filePath, out string response)
        {
            if (!ValidateMediaFile(filePath))
            {
                response = "Invalid media file : " + filePath;
                Debug.LogError(response);
                return false;
            }

            bool success = UploadMedia(filePath, out response);
            if (!success)
            {
                response = "Failed to upload media: " + response;
                Debug.LogError(response);
                return false;
            }

            string mediaID = Regex.Match(response, @"(\Dmedia_id\D\W)(\d*)").Groups[2].Value;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("status", status);
            parameters.Add("media_ids", mediaID);

            WWWForm form = new WWWForm();
            form.AddField("status", status);
            form.AddField("media_ids", mediaID);

            Hashtable headers = GetHeaders(PostTweetURL, parameters);
            return ApiRequest(PostTweetURL, form, headers, out response);
        }

        /// <summary>
        /// Checks to ensure a media file is valid for posting to Twitter.
        /// This will check that the file exists and that it is not currently being
        /// accessed by another process. If the file does not exist yet or if there 
        /// is another process acessing the file
        /// this method will wait for a period before returning false. The idea is that
        /// the process may still be writing the file.
        /// </summary>
        /// <param name="filename">The file to validate</param>
        /// <param name="timeout">The number of seconds to wait before returning false. Defaults to 10 seconds.</param>
        /// <returns>True if this is a valid file otherwise false.</returns>
        private static bool ValidateMediaFile(string filename, int timeout = 10)
        {
            bool isValid = false;
            long endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + timeout;
            while (!File.Exists(filename) && DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= endTime)
            {
                // Waiting for file to be created
            }

            while (!isValid && DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= endTime)
            {
                try
                {
                    Stream stream = new FileStream(filename, FileMode.Open);
                    isValid = true;
                    stream.Close();
                }
                catch
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Handle a request to the API.
        /// </summary>
        /// <param name="url">The URL to make the request to.</param>
        /// <param name="form">The form data to submit.</param>
        /// <param name="headers">The headers to submit.</param>
        /// <param name="response">The response from the request.</param>
        /// <returns>True if no error was returned.</returns>
        private static bool ApiRequest(string url, WWWForm form, Hashtable headers, out string response)
        {
            WWW www = new WWW(url, form.data, headers);
            do { } while (!www.isDone);

            if (string.IsNullOrEmpty(www.error))
            {
                string error = Regex.Match(www.text, @"<error>([^&]+)</error>").Groups[1].Value;
                if (!string.IsNullOrEmpty(error))
                {
                    response = string.Format("Twitter API request failed: {0}", error);
                    Debug.LogError(response);
                    return false;
                }
                else
                {
                    response = www.text;
                    return true;
                }
            }
            else
            {
                response = string.Format("Twitter API request failed: {0} {1}", www.error, www.text);
                Debug.LogError(response);
                return false;
            }
        }

        /// <summary>
        /// Get the required headers for accesing the API.
        /// </summary>
        /// <param name="url">The URL we want to access</param>
        /// <param name="parameters">The parameters to go into the headers</param>
        /// <returns>A Hashtable of the headers.</returns>
        private static Hashtable GetHeaders(string url, Dictionary<string, string> parameters)
        {
            var headers = new Hashtable();
            headers["Authorization"] = OAuthHelper.GetHeaderWithAccessToken("POST",
                url,
                EditorPrefs.GetString(EDITOR_PREFS_TWITTER_API_KEY),
                EditorPrefs.GetString(EDITOR_PREFS_TWITTER_API_SECRET),
                EditorPrefs.GetString(EDITOR_PREFS_TWITTER_ACCESS_TOKEN),
                EditorPrefs.GetString(EDITOR_PREFS_TWITTER_ACCESS_SECRET),
                parameters);
            return headers;
        }

        /// <summary>
        /// Upload a media file to twitter.
        /// </summary>
        /// <param name="filePath">The fully qualified path to the media file to upload.</param>
        /// <param name="response">The response from Twitter.</param>
        /// <returns>True or false depending on the result of the upload</returns>
        private static bool UploadMedia(string filePath, out string response)
        {
            //string status = "Testing media upload to twitter";
            Dictionary<string, string> mediaParameters = new Dictionary<string, string>();
            string mediaString = System.Convert.ToBase64String(File.ReadAllBytes(filePath));
            mediaParameters.Add("media_data", mediaString);
            //mediaParameters.Add("status", status);

            WWWForm mediaForm = new WWWForm();
            mediaForm.AddField("media_data", mediaString);
            //mediaForm.AddField("status", status);
            
            Hashtable mediaHeaders = GetHeaders(UploadMediaURL, mediaParameters);
            mediaHeaders.Add("Content-Transfer-Encoding", "base64");

            return ApiRequest(UploadMediaURL, mediaForm, mediaHeaders, out response);
        }
    }
}