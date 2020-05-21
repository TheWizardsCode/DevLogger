using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WizardsCode.DevLogger
{
    public class DevLogScreenCapture : ScriptableObject
    {
        public enum ImageEncoding { gif, png }

        [SerializeField]
        private ImageEncoding _encoding;
        public ImageEncoding Encoding {
            get { return _encoding; }
            set { _encoding = value; }
        }

        [SerializeField]
        private string _productName;
        [SerializeField]
        private string _version;
        [SerializeField]
        private long _timestamp;
        [SerializeField]
        private string _sceneName;
        [SerializeField]
        public int _width;
        [SerializeField]
        public int _height;

        public bool IsImageSaved = false;
        public bool IsSelected = false;

        private Texture2D _texture;
        public Texture2D Texture
        {
            get {
                if (_texture == null && IsImageSaved)
                {
                    LoadPreviewTexture();
                }
                return _texture; 
            }
            set {
                _texture = value;
            }
        }

        private void LoadPreviewTexture()
        {
            Uri uri = new Uri(GetAbsolutePreviewImagePath());
            string absoluteUri = uri.AbsoluteUri;
            WWW www = new WWW(absoluteUri);
            do { } while (!www.isDone && string.IsNullOrEmpty(www.error));

            if (string.IsNullOrEmpty(www.error))
            {
                _texture = new Texture2D(_width, _height);
                www.LoadImageIntoTexture(_texture);
            } else
            {
                Debug.LogError("Unable to read image from " + uri + " into texture: " + www.error);
            }
        }

        void Awake()
        {
            _productName = Application.productName;
            _version = Application.version;
            _timestamp = DateTime.Now.ToFileTime();
            _sceneName = SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Get the absolute filepath and filename to the image for this capture.
        /// </summary>
        /// <returns></returns>
        public string GetAbsoluteImagePath()
        {
            return GetAbsoluteImageFolder() + Filename;
        }

        /// <summary>
        /// Get the absolute filepath and filename to the preview image for this capture.
        /// </summary>
        /// <returns></returns>
        public string GetAbsolutePreviewImagePath()
        {
            return GetAbsoluteImageFolder() + Filename.Replace(".gif", ".png");
        }

        /// <summary>
        /// Get the path to the folder in which the project is stored
        /// </summary>
        /// <returns></returns>
        public string GetAbsoluteImageFolder()
        {
            string projectPath = Application.dataPath;
            projectPath = projectPath.Replace("Assets", "");
            return projectPath + GetRelativeImageFolder();
        }

        public string GetRelativeImageFolder()
        {
            string relativePath = "DevLog/";
            Directory.CreateDirectory(relativePath);
            return relativePath;
        }

        /// <summary>
        /// Get the relative filepath and filename to the image for this capture.
        /// </summary>
        /// <returns></returns>
        public string GetRelativeImagePath()
        {
            return GetRelativeImageFolder() + Filename;
        }

        public string Filename
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(name);
                sb.Append("_");
                sb.Append(_productName);
                sb.Append("_");
                sb.Append(_sceneName);
                sb.Append("_v");
                sb.Append(_version);
                sb.Append("_");
                sb.Append(_timestamp);
                sb.Append(".");
                sb.Append(Encoding);
                return sb.ToString();
            }
        }
    }
}
