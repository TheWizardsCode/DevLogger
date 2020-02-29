using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine.SceneManagement;
using System;
using WizardsCode.DevLog;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WizardsCode.uGIF
{
	public class CaptureScreen : MonoBehaviour
	{
        public float frameRate = 15;
		public bool isCapturing;
		public int downscale = 1;
		public float duration = 10;
		public bool useBilinearScaling = true;

		public enum Format {  gif, png }

		[System.NonSerialized]
		public byte[] bytes = null;

		List<Image> frames = new List<Image>();
		float period;
		float T = 0;
		float startTime = 0;
		DevLogScreenCapture currentScreenCapture;
		private Texture2D gameViewColorBuffer;
		private bool isEncoding;

		/// <summary>
		/// Configure the capture device ready for the next capture.
		/// Call this immediately before capturing a still or animated GIF.
		/// </summary>
		private void InitializeCapture(ref DevLogScreenCapture screenCapture)
		{
			currentScreenCapture = screenCapture;
			gameViewColorBuffer = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

			period = 1f / frameRate;
			frames = new List<Image>();
			startTime = Time.time;
		}

		/// <summary>
		/// Capture an animated GIF. Note that the application needs to be running for this to work.
		/// </summary>
		public void CaptureAnimatedGIF(ref DevLogScreenCapture screenCapture)
		{
			if (Application.isPlaying)
			{
				InitializeCapture(ref screenCapture);
				isCapturing = true;
			} else
			{
				Debug.LogError("Called CaptureAnimatedGIF when the application is not running. This is not supported, start the application first.");
			}
		}

		/// <summary>
		/// Capture the game camera at this moment in time and save it to a PNG file.
		/// </summary>
		/// <returns>The full path to the saved file.</returns>
		public void CaptureScreenshot(ref DevLogScreenCapture screenCapture)
		{
			InitializeCapture(ref screenCapture);
			StartCoroutine(CaptureFrame());
			StartCoroutine(WriteToDisk());
		}

        /// <summary>
        /// Encode all frames into an animated GIF.
        /// </summary>
        public void EncodeAsAnimatedGIF ()
		{
			bytes = null;
			isEncoding = true;
			Thread thread = new Thread(_Encode);
			thread.Start ();
			StartCoroutine(WriteToDisk());
		}

		/// <summary>
		/// Wait until the image bytes are available and then write them to disk.
		/// <param name="format">The format of the file to write.</param>
		/// </summary>
		IEnumerator WriteToDisk()
		{
			while (bytes == null && isEncoding) yield return null;
			if (currentScreenCapture.Encoding == DevLogScreenCapture.ImageEncoding.gif)
			{
				System.IO.File.WriteAllBytes(currentScreenCapture.GetAbsoluteImagePath(), bytes);
			}

			while (currentScreenCapture.Texture == null) yield return null;
			System.IO.File.WriteAllBytes(currentScreenCapture.GetAbsoluteImagePathForPreview(),
				currentScreenCapture.Texture.EncodeToPNG());

			bytes = null;
			currentScreenCapture.IsImageSaved = true;
		}

		public void _Encode ()
		{
			isEncoding = true;

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
				if (downscale != 1)
				{
					if (useBilinearScaling)
					{
						f.ResizeBilinear(f.width / downscale, f.height / downscale);
					}
					else
					{
						f.Resize(downscale);
					}
				}
				f.Flip ();
				ge.AddFrame (f);
			}
			ge.Finish ();
			bytes = stream.GetBuffer ();
			stream.Close ();

			isEncoding = false;
		}

		/// <summary>
		/// Capture a single frame including all cameras.
		/// </summary>
		IEnumerator CaptureFrame()
		{
			yield return new WaitForEndOfFrame();
			currentScreenCapture.Texture = ScreenCapture.CaptureScreenshotAsTexture();
			frames.Add(new Image(currentScreenCapture.Texture));
		}

		void OnPostRender ()
		{
#if UNITY_EDITOR
			
			if (isCapturing) {
				T += Time.deltaTime;
				if (T >= period)
				{
					T = 0;
					//StartCoroutine(CaptureFrame());
					CaptureGameWindow();
				}
				if (Time.time > (startTime + duration))
				{
					isCapturing = false;
					EncodeAsAnimatedGIF();
				}
			}
		}

		private void CaptureGameWindow()
		{
			EditorWindow window = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
			int heightOffset = 18;
			int width = (int)window.position.width;
			int height = (int)window.position.height - 18;
			Vector2 position = window.position.position;
			position.y += heightOffset;

			//Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(position, width, height);

			Texture2D windowTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
			windowTexture.Apply();
			//windowTexture.SetPixels(pixels);

			windowTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);

			currentScreenCapture.Texture = windowTexture;
			frames.Add(new Image(windowTexture));
		}
#endif
	}
}