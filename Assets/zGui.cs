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
    // Use this for initialization
    void Start()
    {
        XmlDocument xDoc = new XmlDocument();
        xDoc.LoadXml(textAsset.text);
        TrackNode tree = new TrackNode(null, xDoc.DocumentElement);

    }


    // Update is called once per frame
    void Update()
    {

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
    none, song, trackcontainer, track, bbtco, instrumenttrack, instrument, eldata, chordcreator, arpeggiator, midiport, pattern, note
}

public class TrackNode
{
    XmlNode xmlNode;
    public TrackType tracktype = TrackType.none;
    public string name;
    public TrackNode parent;
    public List<TrackNode> childs;
    public Dictionary<string, string> attributes;

    public TrackNode(TrackNode Parent, XmlNode xmlNode)
    {
        this.parent = Parent;
        this.xmlNode = xmlNode;
        name = xmlNode.Name;

        attributes = new Dictionary<string, string>();
        if (xmlNode.Attributes != null) foreach (XmlAttribute attrib in xmlNode.Attributes) attributes.Add(attrib.Name, attrib.Value);

        try
        {
            tracktype = (TrackType)Enum.Parse(typeof(TrackType), name, true);
        }
        catch (Exception e)
        {
        }

        if (tracktype != TrackType.none || name == "multimedia-project") BuildTree();
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


}
