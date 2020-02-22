using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

using System.Threading;
using System.Text;
using UnityEngine.SceneManagement;
using System;

namespace uGIF
{
	public class CaptureScreen : MonoBehaviour
	{
		public float frameRate = 15;
		public bool capture;
		public int downscale = 1;
		public float duration = 10;
		public bool useBilinearScaling = true;
		public List<Texture2D> latestImages;
		public List<string> latestImageFilepaths;

		public enum Format {  gif, png }

		[System.NonSerialized]
		public byte[] bytes = null;
		private int maxImagesToRemember = 4;

		void Start ()
		{
			Directory.CreateDirectory(GetProjectFilepath());
			InitializeCapture();
		}

		/// <summary>
		/// Configure the capture device ready for the next capture.
		/// Call this immediately before capturing a still or animated GIF.
		/// </summary>
		private void InitializeCapture()
		{
			// make space in the latestImage arrays
			latestImages.Insert(0, null);
			latestImageFilepaths.Insert(0, null);
			if (latestImages.Count > maxImagesToRemember)
			{
				latestImages.RemoveAt(latestImages.Count - 1);
				latestImageFilepaths.RemoveAt(latestImageFilepaths.Count - 1);
			}

			GenerateFilepath();
			period = 1f / frameRate;
			colorBuffer = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
			frames = new List<Image>();
			startTime = Time.time;
		}

		/// <summary>
		/// Capture an animated GIF. Note that the application needs to be running for this to work.
		/// </summary>
		public void CaptureAnimatedGIF()
		{
			if (Application.isPlaying)
			{
				InitializeCapture();
				capture = true;
			} else
			{
				Debug.LogError("Called CaptureAnimatedGIF when the application is not running. This is not supported, start the application first.");
			}
		}

		/// <summary>
		/// Capture the game camera at this moment in time and save it to a PNG file.
		/// </summary>
		/// <returns>The full path to the saved file.</returns>
		public string SaveScreenshot()
		{
			InitializeCapture();
			StartCoroutine(CaptureFrame(Format.png));
			bytes = currentScreenshotAsTexture.EncodeToPNG();
			StartCoroutine(WriteToDisk(Format.png));
			return currentFilepath + ".png";
		}

		/// <summary>
		/// Get the path to the folder in which images and GIFs will be stored.
		/// </summary>
		/// <returns></returns>
		public string GetProjectFilepath()
		{
			string projectPath = Application.dataPath;
			projectPath = projectPath.Replace("Assets", "");
			return projectPath;
		}

		/// <summary>
		/// Encode all frames into an animated GIF.
		/// </summary>
		public void EncodeAsAnimatedGIF ()
		{
			bytes = null;
			Thread thread = new Thread(_Encode);
			thread.Start ();
			StartCoroutine(WriteToDisk(Format.gif));
		}

		/// <summary>
		/// Wait until the image bytes are available and then write them to disk.
		/// <param name="format">The format of the file to write.</param>
		/// </summary>
		IEnumerator WriteToDisk(Format format)
		{
			while (bytes == null) yield return null;
			
			System.IO.File.WriteAllBytes(currentFilepath + "." + format, bytes);
			bytes = null;
		}

		private void GenerateFilepath()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(GetProjectFilepath());
			sb.Append("DevLog/");
			sb.Append(Application.productName);
			sb.Append("_");
			sb.Append(SceneManager.GetActiveScene().name);
			sb.Append("_v");
			sb.Append(Application.version);
			sb.Append("_");
			sb.Append(DateTime.Now.ToFileTime());
			currentFilepath = sb.ToString();
		}

		/// <summary>
		/// Get the filepath to the image at the supplied index of the
		/// latest captured images.
		/// </summary>
		/// <param name="idx">The index of required image in the latest images list</param>
		/// <returns></returns>
		public string GetLatestImagePath(int idx)
		{
			return latestImageFilepaths[idx];
		}

		public void _Encode ()
		{
			capture = false;

			var ge = new GIFEncoder ();
			ge.useGlobalColorTable = true;
			ge.repeat = 0;
			ge.FPS = frameRate;
			ge.transparent = new Color32 (255, 0, 255, 255);
			ge.dispose = 1;

			var stream = new MemoryStream ();
			ge.Start (stream);
			Image f;
			for (int i = 0; i < frames.Count; i++) {
				f = frames[i];
				if (downscale != 1) {
					if(useBilinearScaling) {
						f.ResizeBilinear(f.width/downscale, f.height/downscale);
					} else {
						f.Resize (downscale);
					}
				}
				f.Flip ();
				ge.AddFrame (f);
			}
			ge.Finish ();
			bytes = stream.GetBuffer ();
			stream.Close ();
		}

		/// <summary>
		/// Capture a single frame including all cameras.
		/// </summary>
		IEnumerator CaptureFrame(Format format)
		{
			yield return new WaitForEndOfFrame();
			currentScreenshotAsTexture = ScreenCapture.CaptureScreenshotAsTexture();
			StorePreProcessedFrame(currentScreenshotAsTexture, format);
		}

		/// <summary>
		/// Store the frame ready for processing.
		/// </summary>
		/// <param name="texture">The image to store, in the form of a texture.</param>
		/// <param name="format">The format the image will eventually be saved in.</param>
		private void StorePreProcessedFrame(Texture2D texture, Format format)
		{
			frames.Add(new Image(texture));
			latestImages[0] =  texture;
			latestImageFilepaths[0] = currentFilepath + "." + format;
		}

		void OnPostRender ()
		{
			if (capture) {
				T += Time.deltaTime;
				if (T >= period)
				{
					T = 0;
					StartCoroutine(CaptureFrame(Format.gif));
				}
				if (Time.time > (startTime + duration))
				{
					capture = false;
					EncodeAsAnimatedGIF();
				}
			}
		}

		List<Image> frames = new List<Image> ();
		Texture2D colorBuffer;
		float period;
		float T = 0;
		float startTime = 0;
		private static string currentFilepath;
		private Texture2D currentScreenshotAsTexture;

	}
}