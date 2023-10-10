using System.Collections.Generic;
using System;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System.Linq;

public class NewBehaviourScript : MonoBehaviour
{
    MidiFile midi;
    TempoMap tempoMap;
    ValueChange<Tempo>[] tempoChanges;
    Note[] notes;
    Transform noteInitPoint;
    GameObject[] noteInstances;
    GameObject noteObj;
    public float scale = 1.0f;
    float scrollSpeedByTempo;
    static InputDevice _inputDevice;

    bool isPlay = false;

    void Start()
    {
        noteInitPoint = GameObject.Find("Notes").transform;
        midi = MidiFile.Read($"{Application.dataPath}/Midis/b9IsAwesome.mid");
        tempoMap = midi.GetTempoMap();
        tempoChanges = tempoMap.GetTempoChanges().ToArray();
        scrollSpeedByTempo = 500000.0f / tempoChanges[0].Value.MicrosecondsPerQuarterNote;
        notes = new Note[midi.GetNotes().Count];
        midi.GetNotes().CopyTo(notes, 0);
        Debug.Log($"Note Count: {notes.Length}");
        noteInstances = new GameObject[notes.Length];
        noteObj = Resources.Load("Prefabs/Note") as GameObject;
        for (int i = 0; i < notes.Length; i++)
        {
            noteInstances[i] = Instantiate(noteObj, noteInitPoint);
            noteInstances[i].transform.localScale = new Vector3(0.5f, 0.5f, (notes[i].EndTime - notes[i].Time) / 480.0f * scale);
            noteInstances[i].transform.position = new Vector3(notes[i].NoteNumber * 0.5f - 30.0f, 0, (((notes[i].EndTime - notes[i].Time) / 2.0f) + notes[i].Time) / 480.0f * scale);
        }

        try
        {
            _inputDevice = InputDevice.GetByName("Digital Piano");
            _inputDevice.EventReceived += OnEventReceived;
            _inputDevice.StartEventsListening();
            Debug.Log(_inputDevice.IsListeningForEvents);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    void Update()
    {
        if (isPlay)
            ScrollNotes();
    }

    public void PlayMidi()
    {
        isPlay = true;
    }

    void ScrollNotes()
    {
        noteInitPoint.Translate(new Vector3(0, 0, -2 * scrollSpeedByTempo * scale * Time.deltaTime));
    }

    static void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        if (e.Event.EventType != MidiEventType.ActiveSensing)
            Debug.Log($"{midiDevice.Name} {e.Event}");
    }
}
