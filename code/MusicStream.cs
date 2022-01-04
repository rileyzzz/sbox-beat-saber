using System;
using NVorbis;
using AForge.Math;
using Sandbox;
using System.Collections.Generic;

public class MusicStream
{

	//const int bufferSize = 4096; // around 20ms window
	//public static readonly int ChannelBufferSize = 1024;
	//public static readonly int ChannelBufferSize = 512;
	public static readonly int ChannelBufferSize = 512;

	//const int channelBufferSize = 4096;
	public struct VisualizerWindow
	{
		public float[] Data;

		//public VisualizerWindow(Complex[] data)
		//{
		//	Data = new float[data.Length];
		//	for ( int i = 0; i < data.Length; i++ )
		//	//Data[i] = (float)(data[i].Re * 1000.0);
		//	//Data[i] = (float)(data[i].Re);
		//	{
		//		var re = data[i].Re;
		//		var im = data[i].Im;
		//		//Data[i] = (((int)Math.Sqrt( im * im ) / channelBufferSize / 2) % 100);
		//		//Data[i] = (((int)Math.Abs( im ) / channelBufferSize / 2) % 100);
		//		Data[i] = (float)Math.Abs( im );
		//	}
		//}

		public VisualizerWindow( float[] data )
		{
			Data = (float[])data.Clone();
		}

		public float[] GetFFTData1D()
		{
			Complex[] transformData = new Complex[Data.Length];
			for ( int i = 0; i < Data.Length; i++ )
				transformData[i] = new Complex( (double)Data[i], 0.0 );

			FourierTransform.FFT( transformData, FourierTransform.Direction.Backward );

			float[] outData = new float[Data.Length];
			for ( int i = 0; i < Data.Length; i++ )
			{
				var re = transformData[i].Re;
				var im = transformData[i].Im;
				outData[i] = (float)Math.Abs( im );
			}

			return outData;
		}

		public void GetFFTData2D(out float[] x, out float[] y)
		{
			x = GetFFTData1D();
			y = Data;
		}
	}

	string Path;
	VorbisReader reader;
	List<VisualizerWindow>[] channelWindows;

	Sound sound;
	SoundStream stream;

	double startTime = 0.0f;

	public MusicStream(string path)
	{
		Path = path;

		var fs = FileSystem.Data;
		if ( !fs.FileExists( Path ) )
			Path = Path.Substring(0, Path.Length - 4) + ".ogg";
		
		if ( !fs.FileExists( Path ) )
			Log.Error( "Unable to find file " + Path );

		reader = new VorbisReader( Path );

		Log.Info( "Loaded OGG file." );

		Log.Info( "Channels: " + reader.Channels );
		Log.Info( "Sample Rate: " + reader.SampleRate );
		Log.Info( "Length: " + reader.TotalTime );

		channelWindows = new List<VisualizerWindow>[reader.Channels];
		//ReadAudio();
	}

	//void ReadAudio()
	//{
	//	using ( var vorbis = new NVorbis.VorbisReader( Path ) )
	//	{
	//		Log.Info( "Loaded OGG file." );

	//		Log.Info( "Channels: " + vorbis.Channels );
	//		Log.Info( "Sample Rate: " + vorbis.SampleRate );
	//		Log.Info( "Length: " + vorbis.TotalTime );

	//		//int bufferSize = vorbis.Channels * vorbis.SampleRate / 5; // 200ms
	//		int bufferSize = 17640; // 200ms
	//		Samples = new float[vorbis.TotalSamples];

	//		var readBuffer = new float[bufferSize];
	//		int cnt;
	//		while ( (cnt = vorbis.ReadSamples( readBuffer, 0, bufferSize )) > 0 )
	//		{
				
	//		}

			
	//		vorbis.ReadSamples( Samples, 0, vorbis.TotalSamples );
	//	}
	//}

	short ConvertSample( float sample )
	{
		return (short)(sample * short.MaxValue);
	}

	public void Play()
	{
		Log.Info("Playing " + Path);

		for ( int i = 0; i < channelWindows.Length; i++ )
			channelWindows[i] = new List<VisualizerWindow>();

		sound = Sound.FromScreen( "audiostream.default" );
		stream = sound.CreateStream( reader.SampleRate, reader.Channels );
		stream.SetVolume(0.5f);
		//startTime = Time.Sound;

		//int bufferSize = vorbis.Channels * vorbis.SampleRate / 5; // 200ms
		
		int bufferSize = reader.Channels * ChannelBufferSize; // 200ms

		var readBuffer = new float[bufferSize];
		var writeBuffer = new short[bufferSize];

		var deinterleaved_data = new float[reader.Channels][];
		for ( int i = 0; i < deinterleaved_data.Length; i++ )
			deinterleaved_data[i] = new float[ChannelBufferSize];

		int cnt;
		while ( (cnt = reader.ReadSamples( readBuffer, 0, bufferSize )) > 0 )
		{

			//windows.Add( CreateVisualizerWindow( readBuffer ) );

			for ( int i = 0; i < readBuffer.Length; i += reader.Channels )
			{
				for(int j = 0; j < reader.Channels; j++ )
				{
					float sample = readBuffer[i + j];
					writeBuffer[i + j] = ConvertSample( sample );
					deinterleaved_data[j][i / reader.Channels] = sample;
				}
			}

			for ( int i = 0; i < reader.Channels; i++ )
				channelWindows[i].Add( new VisualizerWindow(deinterleaved_data[i]) );

			stream.WriteData( writeBuffer );
		}

		startTime = Time.Sound;
	}

	public void Stop()
	{

	}


	//public float[] GetVisualizerData(int channelIdx)
	//{
	//	//Log.Info("sound time " + stream.QueuedSampleCount );

	//	channelIdx = Math.Min( channelIdx, channelWindows.Length - 1 );

	//	var channel = channelWindows[channelIdx];

	//	//int window = (int)Math.Floor(sound.ElapsedTime * reader.SampleRate / channelBufferSize);
	//	//int window = (int)Math.Floor( (Time.Sound - startTime) * reader.SampleRate / channelBufferSize );
	//	int window = (int)Math.Floor( (float)(reader.TotalSamples - stream.QueuedSampleCount) / ChannelBufferSize );

	//	//Log.Info( "sound overall " + sound.ElapsedTime / reader.TotalTime.TotalSeconds );
	//	//Log.Info( "window overall " + (float)window / (float)channel.Count );

	//	//int window = (int)Math.Floor((Time.Sound - startTime) * reader.SampleRate / channelBufferSize);

	//	if( window < 0 || window >= channel.Count)
	//	{
	//		Log.Info( "nonexistent window (size " + channel.Count + ")" );
	//		return new float[0];
	//	}

	//	//Log.Info("window " + window);
	//	//return channel[window].Data;
	//	return channel[window].GetFFTData1D();
	//}

	//S&box resamples to 44100hz
	static readonly int sbox_samplerate = 44100;

	public int SamplesElapsed => (int)(reader.TotalSamples * (sbox_samplerate / (float)reader.SampleRate) - stream.QueuedSampleCount);
	public float TimeElapsed => (float)SamplesElapsed / sbox_samplerate;

	//unresampled
	public float OriginalSamplesElapsed => (reader.TotalSamples - stream.QueuedSampleCount * ((float)reader.SampleRate / sbox_samplerate));

	public VisualizerWindow? GetVisualizerWindow( int channelIdx )
	{
		channelIdx = Math.Min( channelIdx, channelWindows.Length - 1 );

		var channel = channelWindows[channelIdx];

		//int window = (int)Math.Floor( (float)(reader.TotalSamples - stream.QueuedSampleCount) / ChannelBufferSize );
		int window = (int)Math.Floor( OriginalSamplesElapsed / ChannelBufferSize );

		if ( window < 0 || window >= channel.Count )
		{
			Log.Info( "nonexistent window (size " + channel.Count + ")" );
			return null;
		}

		return channel[window];
	}

	//public float TimeElapsed => (float)(reader.TotalSamples - stream.QueuedSampleCount) / reader.SampleRate;
	//public float TimeElapsed => (float)(reader.TotalSamples * (44100 / (float)reader.SampleRate) - stream.QueuedSampleCount) / 44100;
}
