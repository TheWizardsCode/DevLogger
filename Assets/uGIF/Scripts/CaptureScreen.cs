using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine.SceneManagement;
using System;
using WizardsCode.DevLog;

namespace WizardsCode.uGIF
{
	public class CaptureScreen : MonoBehaviour
	{
        public float frameRate = 15;
		public bool isCapturing;
		public int width = 320; // the width of the final image. The height will be adjust to maintain aspect ratio
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
			if (currentScreenCapture.Encoding == DevLogScreenCapture.ImageEncoding.gif)
			{
				while (bytes == null) yield return null;
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
			isCapturing = false;

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
				if (width != f.width) {
					int scale = width / f.width;
					if(useBilinearScaling) {
						f.ResizeBilinear(f.width * scale, f.height * scale);
					} else {
						f.Resize (width);
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
		IEnumerator CaptureFrame()
		{
			yield return new WaitForEndOfFrame();
			currentScreenCapture.Texture = ScreenCapture.CaptureScreenshotAsTexture();
			frames.Add(new Image(currentScreenCapture.Texture));
		}

		void OnPostRender ()
		{
			if (isCapturing) {
				T += Time.deltaTime;
				if (T >= period)
				{
					T = 0;
					StartCoroutine(CaptureFrame());
				}
				if (Time.time > (startTime + duration))
				{
					isCapturing = false;
					EncodeAsAnimatedGIF();
				}
			}
		}
	}
}