using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Social
{
    [ExecuteInEditMode]
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
        private const string VerifyCredentialsURL = "https://api.twitter.com/1.1/account/verify_credentials.json";

        private static float verifyCredentialsTime = 0;

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

        private static bool VerifyCredentials()
        {
#if UNITY_EDITOR
            float time = (float)EditorApplication.timeSinceStartup;
#else
                float time = Time.time;
#endif
            if (time > verifyCredentialsTime)
            {
                verifyCredentialsTime = time + 2;
                Hashtable headers = GetHeaders(VerifyCredentialsURL, new Dictionary<string, string>());
                if (ApiRequest(VerifyCredentialsURL, new WWWForm(), headers, out string response))
                {
                    EditorPrefs.SetString(EDITOR_PREFS_TWITTER_ACCESS_TOKEN, "");
                    EditorPrefs.SetString(EDITOR_PREFS_TWITTER_ACCESS_SECRET, "");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
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

            if (!VerifyCredentials())
            {
                response = "Twitter credentials are invalid.";
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

        static string mediaIDs;
        public static bool PublishTweetWithMedia(string status, List<string> filePaths, out string response)
        {
            if (filePaths.Count > 4)
            {
                response = "Error sending Tweet: Can only attach four images to a tweet.";
                Debug.LogError(response);
                return false;
            }

            mediaIDs = "";
            for (int i = 0; i < filePaths.Count; i++)
            {
                if (!ValidateMediaFile(filePaths[i]))
                {
                    response = "Invalid media file : " + filePaths;
                    Debug.LogError(response);
                    return false;
                }

                bool success = UploadMedia(filePaths[i], out response);
                if (!success)
                {
                    response = "Failed to upload media: " + response;
                    Debug.LogError(response);
                    return false;
                }

                if (string.IsNullOrEmpty(mediaIDs))
                {
                    mediaIDs = Regex.Match(response, @"(\Dmedia_id\D\W)(\d*)").Groups[2].Value;
                }
                else
                {
                    mediaIDs += "," + Regex.Match(response, @"(\Dmedia_id\D\W)(\d*)").Groups[2].Value;
                }
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("status", status);
            parameters.Add("media_ids", mediaIDs);

            WWWForm form = new WWWForm();
            form.AddField("status", status);
            form.AddField("media_ids", mediaIDs);

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
                response += "\n\n";
                response += "headers:\n " + headers;
                response += "form: \n" + form.ToString();
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
            //mediaParameters.Add("media_type", "image/png");
            
            WWWForm mediaForm = new WWWForm();
            mediaForm.AddField("media_data", mediaString);
            //mediaForm.AddField("media_type", "image/png");
            
            Hashtable mediaHeaders = GetHeaders(UploadMediaURL, mediaParameters);
            mediaHeaders.Add("Content-Transfer-Encoding", "base64");

            return ApiRequest(UploadMediaURL, mediaForm, mediaHeaders, out response);
        }
    }
}