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
	public class CaptureToGIF : MonoBehaviour
	{
		public float frameRate = 15;
		public bool capture;
		public int downscale = 1;
		public float duration = 10;
		public bool useBilinearScaling = true;
		public string filepath = "Screenshots/";

		[System.NonSerialized]
		public byte[] bytes = null;

		void Start ()
		{
			filepath = "Screenshots/";
			Directory.CreateDirectory(filepath);
			InitializeCapture();
		}

		private void InitializeCapture()
		{
			period = 1f / frameRate;
			colorBuffer = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
			frames = new List<Image>();
			startTime = Time.time;
		}

		public void StartCapturing()
		{
			InitializeCapture();
			capture = true;
		}

		public void HiResScreenShot()
		{
			InitializeCapture();
			StartCoroutine(RecordFrame(true));
		}

		public bool IsDone
		{
			get { return !capture; }
		}

		public void Encode ()
		{
			bytes = null;
			var thread = new Thread (_Encode);
			thread.Start ();
			StartCoroutine(WaitForBytes());
		}

		IEnumerator WaitForBytes() {
			while(bytes == null) yield return null;
			
			StringBuilder filename = new StringBuilder();
			filename.Append(Application.productName);
			filename.Append("_");
			filename.Append(SceneManager.GetActiveScene().name);
			filename.Append("_v");
			filename.Append(Application.version);
			filename.Append("_");
			filename.Append(DateTime.Now.ToFileTime());
			filename.Append(".gif");


			System.IO.File.WriteAllBytes (filepath + filename, bytes);
			bytes = null;
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
			foreach (var f in frames) {
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
		/// Capture a single frame including all cameras. Optionally encode this as a give and save
		/// it to a file.
		/// </summary>
		/// <param name="encode">If set to true the frame will be immediately encoded and saved as a GIF.</param>
		/// <returns></returns>
		IEnumerator RecordFrame(bool encode = false)
		{
			yield return new WaitForEndOfFrame();
			frames.Add(new Image(ScreenCapture.CaptureScreenshotAsTexture()));
			if (encode)
			{
				Encode();
			}
		}

		void OnPostRender ()
		{
			if (capture) {
				T += Time.deltaTime;
				if (T >= period)
				{
					T = 0;
					StartCoroutine(RecordFrame(false));
				}
				if (Time.time > (startTime + duration))
				{
					capture = false;
					Encode();
				}
			}
		}

		List<Image> frames = new List<Image> ();
		Texture2D colorBuffer;
		float period;
		float T = 0;
		float startTime = 0;
        
	}
}