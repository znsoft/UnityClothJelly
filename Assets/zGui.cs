using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;


public class zGUI : MonoBehaviour
{
	public string TextLogin;
	Vector2 scrollPosition = new Vector2(0, 0);
	public TextAsset textAsset;
	public double gain = 0.05;
	private double increment;
	private double phase;
	public double sampling_frequency = 48000;
	double[] freq = { 440, 440, 0, 400, 450, 450, 470,350 };
	int OneSecTickLen = 1, TickCounter;
	int pos;
	zSequence seq;

	// Use this for initialization
	void Start()
	{
		OneSecTickLen = (int)(sampling_frequency / 2);
		pos = 0;
		sampling_frequency = (double)AudioSettings.outputSampleRate;
		// read music file
		XmlDocument xDoc = new XmlDocument();
		xDoc.LoadXml(textAsset.text);
		TrackNode tree = new TrackNode(null, xDoc.DocumentElement);
		int len = TrackNode.maxpos;
		foreach(var child in tree.childs)child.CalcSong();

		seq = tree.seq;
		Debug.Log (seq.sequence.Length);
		
	}
	
	
	// Update is called once per frame
	void Update()
	{
		
	}
	
	
	double FM(double t, double f, double p)
	{
		double PI2 = Math.PI * 2;
		return Math.Sin(f * PI2 * t + (PI2 + PI2 * Math.Sin(t / PI2))
		                * Math.Sin(p * f * PI2 * t)// FM Modulation
		                );
		
	}
	
	void OnAudioFilterRead(float[] data, int channels)
	{
		
		int currentlen = data.Length;
		
		
		
		double frequency = freq[pos % freq.Length];
		
		// update increment in case frequency has changed
		increment = frequency * 2 * Math.PI / sampling_frequency;
		
		
		
		for (var i = 0; i < data.Length; i = i + channels)
		{
			TickCounter++;
			double d = ((double)OneSecTickLen - (double)TickCounter) / (double)OneSecTickLen;
			if (TickCounter >= OneSecTickLen)
			{
				pos++;
				TickCounter = 0;
				frequency = freq[pos % freq.Length];
				increment = frequency * 2 * Math.PI / sampling_frequency;
			}
			phase = phase + increment;
			// this is where we copy audio data to make them “available” to Unity
			data[i] = (float)(gain * d * FM(phase, frequency, gain));//(float)(gain * (d) * Math.Sin (phase * (0.51+ d * Math.Cos (phase+ frequency  )))); //* frequency 
			
			data[i + 1] = (float)(gain * (d) * Math.Sin(phase * (0.51 + d * Math.Cos(phase + frequency)))); //* frequency 
			// if we have stereo, we copy the mono data to each channel
			//if (channels == 2)
			//data [i + 1] = data [i];
			if (phase > 2 * Math.PI)
				phase = 0;
		}
	}
	
	
	//var btnTexture : Texture;
	void OnGUI()
	{
		//Screen.height  		Screen.width
		TextLogin = GUI.TextField(new Rect(10, 10, 150, 30), TextLogin);
		GUI.RepeatButton(new Rect(20, 20, 150, 30), "------");
		scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 100, 100),
		                                     scrollPosition, new Rect(0, 0, 220, 200));
		
		
		if (GUI.Button(new Rect(10, 70, 50, 30), TextLogin))
			Debug.Log("Clicked the button with text");
		
		
		// End the scroll view that we began above.
		GUI.EndScrollView();
	}
}

public enum TrackType
{
	none, song, trackcontainer, track, bbtrack, bbtco, pattern, note
}



public class TrackNode
{
	static public int div = 255;
	static public int maxpos = 0;
	public static int PatternsLen = 16;
	static int patternnum = 0;
	XmlNode xmlNode;
	public TrackType tracktype = TrackType.none;
	public string name;
	public TrackNode parent;
	public List<TrackNode> childs;
	public Dictionary<string, string> attributes;
	public int pos;
	public int len;
	public zNote note;
	public zSequence seq;
	public zChannel chan;
	List<zPattern> patterns;

	
	
	public TrackNode(TrackNode Parent, XmlNode xmlNode)
	{
		this.parent = Parent;
		this.xmlNode = xmlNode;
		name = xmlNode.Name;
		
		
		attributes = new Dictionary<string, string>();
		if (xmlNode.Attributes != null) foreach (XmlAttribute attrib in xmlNode.Attributes) attributes.Add(attrib.Name, attrib.Value);
		string p;
		if (attributes.TryGetValue("pos", out p))
			pos = Int32.Parse(p);
		string l;
		if (attributes.TryGetValue("len", out l))
			len = Int32.Parse(l);
		
		try
		{
			tracktype = (TrackType)Enum.Parse(typeof(TrackType), name, true);
		}
		catch (Exception e)
		{
		}
		
		if (tracktype != TrackType.none || parent == null) BuildTree();
		
		CalcSongLen();
		CalcDiv();
		CalcNote();
	}
	
	private void CalcNote()
	{
		if (tracktype != TrackType.note) return;
		note = new zNote();
		string p;
		if (attributes.TryGetValue("key", out p))
			note.note = Int32.Parse(p);
		note.len = len;
		string v;
		//if (attributes.TryGetValue("vol", out v))
		//  note = Int32.Parse(v);
		
	}
	
	private void CalcDiv()
	{
		if (pos > 0)
		{
			float d = 0;
			d = (float)pos / (float)div;
			while (Mathf.Floor(d) != d)
			{
				div--;
				if (div == 1) break;
				d = (float)pos / (float)div;
			}
			
		}
	}
	
	private void CalcSongLen()
	{
		//if (tracktype == TrackType.pattern) {
		int pl = pos + len;
		if (pl > maxpos) maxpos = pl;
		name = name + patternnum.ToString();
		patternnum++;
		//}
	}
	
	private void BuildTree()
	{
		if (!xmlNode.HasChildNodes)
			return;
		childs = new List<TrackNode>();
		foreach (XmlNode xNode in xmlNode.ChildNodes)
		{
			childs.Add(new TrackNode(this, xNode));
		}
	}
	
	public void CalcSong() {
		
		
		switch (tracktype)
		{
		case TrackType.pattern:
			chan = parent.chan;

			int maxpos = 0;
			if(childs == null)return;
			foreach(TrackNode note in childs)
				if (note.pos>maxpos)maxpos = note.pos;

			int maxindex = 1+ maxpos/(div * PatternsLen);
			zPattern[] pattern = new zPattern[maxindex];
			foreach(TrackNode note in this.childs){
				int noteindex = (note.pos/div)%PatternsLen;
				int patindex = note.pos/(div * PatternsLen);
				zPattern pat = pattern[patindex];

				if(pat==null)pat = new zPattern();
				pat.name = name;
				if(pat.notes == null)pat.notes = new zPolyNote[PatternsLen];
				zPolyNote pnotes = pat.notes[noteindex];
				if(pnotes == null)pnotes = new zPolyNote();
				pat.notes[noteindex] = pnotes;
				if(pnotes.note==null)pnotes.note = new List<zNote>();
				pnotes.note.Add(note.note);
				pattern[patindex] = pat;

			}
			patterns = parent.patterns;
			if(patterns == null)patterns = new List<zPattern>();
			chan.patterns = patterns;
			patterns.AddRange(pattern);




			break;
		case TrackType.track:
			chan = new zChannel();
			chan.name = name;
			chan.patterns = new List<zPattern>();
			foreach(var child in childs)child.CalcSong();
			parent.seq.Put(pos,chan);
			break;
		case TrackType.song:
			foreach(var child in childs)child.CalcSong();
			parent.seq = seq;
			//return seq;
			break;
		case TrackType.trackcontainer:
			seq = new zSequence(pos,len,div);
			parent.seq = seq;
			foreach(var child in childs)child.CalcSong();
			break;
		case TrackType.bbtrack:
			
			break;
		case TrackType.bbtco:
			
			break;
		}
		
		//return null;
		
	}
	
	
}
//-----------------------------------------------------



public class zPlayer
{
	int OneSecTickLen = 1;
	int TickCounter;
	int pos;
	
	public zPlayer(int len)
	{
		double sampling_frequency = (double)AudioSettings.outputSampleRate;
		
		OneSecTickLen = (int)(sampling_frequency / 2);
		pos = 0;
		
		
	}
	
	
	void OnAudioFilterRead(float[] data, int channels)
	{
		
		int currentlen = data.Length;
		
		for (var i = 0; i < data.Length; i = i + channels)
		{
			TickCounter++;
			double d = ((double)OneSecTickLen - (double)TickCounter) / (double)OneSecTickLen;
			if (TickCounter >= OneSecTickLen)
			{
				pos++;
				TickCounter = 0;
				
			}
		}
	}
	
	
	
	
}


public class zTimeTick
{
	public List<zChannel> channels;
	
}



public class zSequence
{
	
	int len;
	int div;
	
	public zTimeTick[] sequence;
	public zSequence(int pos, int len, int div)
	{
		this.len = len;/// div;
		this.div = div;
		sequence = new zTimeTick[1+this.len / div];
	}
	
	public void Put(int pos, zChannel channel)
	{
		int index = pos / div;
		zTimeTick s = sequence[index];
		if (s == null)
			s = new zTimeTick();
		if (s.channels == null)
			s.channels = new List<zChannel>();
		if (s.channels.Contains(channel))
			return;
		s.channels.Add(channel);
		sequence[index] = s;
		
	}
	
}

public class zNote
{
	public int note;
	public int len;
}

public class zPolyNote
{
	public List<zNote> note;
	public int vol;
}

public class zPattern
{
	public string name;
	public zPolyNote[] notes;
}


public class zChannel
{
	public string name;
	public List<zPattern> patterns;
}


//--------------------------------------------------
public enum zInstrumentType
{
	Sin, Saw, Sqr, FM
	
	
}

public class zInstrument
{
	zInstrumentType type;
	zSynthGen synth;
	public float length;
	public zInstrument(zInstrumentType type, float sampling_frequency)
	{
		this.type = type;
		synth = new zSynthGen(sampling_frequency);
	}
	
	public void GenSample(float freq, float phase)
	{
		switch (type) {
		case zInstrumentType.Sin:
			synth.GenSinWave(freq, phase);
			break;
		case zInstrumentType.Saw:
			synth.GenSaw(freq, phase);
			break;
		case zInstrumentType.Sqr:
			synth.GenSqr(freq, phase);
			break;
		case zInstrumentType.FM:
			synth.FM(freq, phase);
			break;
		}
		
		
	}
	
	
	
}


//---------Synth
public class zSynthGen
{
	float sampling_frequency;
	float frequency;
	float phase;
	zSynthMath synth;
	float increment;// = frequency * 2 * Math.PI / sampling_frequency;
	public float valueSample;
	
	public zSynthGen(float sampling_frequency)
	{
		synth = zSynthMath.GetInstance();
		this.sampling_frequency = sampling_frequency;
		phase = 0;
	}
	
	public zSynthGen CalcNote(int note)
	{
		calcFrequencyIncrement(synth.GetFrequencyOfNote(note));
		return this;
	}
	
	void calcFrequencyIncrement(float newfreq)
	{
		if (newfreq == frequency)
			return;
		frequency = newfreq;
		increment = frequency * synth.PI2 / sampling_frequency;
	}
	
	zSynthGen getPhase(float newfreq)
	{
		calcFrequencyIncrement(newfreq);
		return getPhase();
	}
	
	zSynthGen getPhase()
	{
		phase += increment;
		return this;
	}
	
	public zSynthGen GenSinWave(float freq, float phase)
	{
		getPhase(freq);
		valueSample = synth.Sin(this.phase + phase);
		return this;
	}
	
	public zSynthGen GenSinWave(float phase)
	{
		return GenSinWave(frequency, phase);
	}
	
	public zSynthGen FM(float phase)
	{
		return FM(frequency, phase);
	}
	
	public zSynthGen FM(float freq, float phase)
	{
		getPhase(freq);
		valueSample = synth.Sin(this.phase + (synth.PI2 + synth.PI2 * synth.Sin(this.phase / synth.PI2)) * synth.Sin(phase * this.phase)// FM Modulation
		                        );
		return this;
	}
	
	public zSynthGen GenSaw(float phase)
	{
		return GenSaw(frequency, phase);
	}
	
	public zSynthGen GenSaw(float freq, float phase)
	{
		getPhase(freq);
		float sawphase = (this.phase + phase) / synth.PI2;
		valueSample = (Mathf.Floor(sawphase) - sawphase) * 2 - 1;
		return this;
	}
	
	public zSynthGen GenSqr(float phase)
	{
		return GenSqr(frequency, phase);
	}
	
	public zSynthGen GenSqr(float freq, float phase)
	{
		getPhase(freq);
		float sqrphase = Mathf.PI * synth.Sin(this.phase);
		valueSample = (sqrphase > phase) ? 1 : -1;
		return this;
	}
	
	float buf;
	public zSynthGen resonance(byte freq, byte q)
	{
		buf = buf + freq * (valueSample - buf + q * (buf - valueSample) / (74 - (freq >> 1))) / 127;
		valueSample = valueSample + freq * (buf - valueSample) / 127;
		return this;
	}
	
	
	
}

//-------------------------------
public class zSynthMath
{
	//float rnd;
	public float PI2 = Mathf.PI * 2;
	static zSynthMath singletone = null;
	public static zSynthMath GetInstance()
	{
		if (singletone == null)
			singletone = new zSynthMath();
		return singletone;
	}
	
	private zSynthMath()
	{
		makenotetab();
		initsintab();
		initHPtab();
		
		//UnityEngine.Random.seed = 0;
	}
	
	float[] notetab = new float[255];
	void makenotetab()
	{
		double e = Math.Pow(2, (1.0 / 12));
		int i;
		double n = 23;
		for (i = 0; i < 255; i++)
			notetab[i] = (float)(n *= e);
	}
	//--------------------------------------------
	float[] sintab = new float[1025];   //------------------------------------------------
	void initsintab()
	{
		for (int i = 0; i <= 1024; i++)
			sintab[i] = Mathf.Sin(i * PI2 / 1023);
		
	}
	
	float[] hpfb = new float[256];
	void initHPtab()
	{
		for (int i = 0; i < 255; i++)
			hpfb[i] = Mathf.Exp(-2.0f * Mathf.PI * i / 255);
	}
	
	public float GetFrequencyOfNote(int note)
	{
		return notetab[note % notetab.Length];
	}
	
	public float Sin(float a)
	{
		int pos = (int)(a * sintab.Length) % sintab.Length;
		return sintab[pos];
	}
	
	public float RND()
	{
		return UnityEngine.Random.Range(-1, 1);
	}
	
}


