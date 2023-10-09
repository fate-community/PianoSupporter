using System.Collections.Generic;
using System;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;

public class NewBehaviourScript : MonoBehaviour
{
    MidiFile midi;
    Note[] notes;
    GameObject[] noteInstances;
    GameObject noteObj;
    public float scale = 1.0f;
    static InputDevice _inputDevice;
    Action<TimedEvent> _midiEvent;

    void Start()
    {
        midi = MidiFile.Read($"{Application.dataPath}/Midis/BUTTERFLY_.mid");
        notes = new Note[midi.GetNotes().Count];
        midi.GetNotes().CopyTo(notes, 0);
        Debug.Log($"Note Count: {notes.Length}");
        TempoMap tempoMap = midi.GetTempoMap();
        noteInstances = new GameObject[notes.Length];
        noteObj = Resources.Load("Prefabs/Note") as GameObject;
        for (int i = 0; i < notes.Length; i++)
        {
            noteInstances[i] = Instantiate(noteObj);
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
        _midiEvent += EventTest;
    }

    public void PlayMidi()
    {
        midi.Play(OutputDevice.GetByIndex(0));
        midi.ProcessTimedEvents(_midiEvent);
    }

    public void EventTest(TimedEvent e)
    {
        Debug.Log("Event");
    }

    static void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        if (e.Event.EventType != MidiEventType.ActiveSensing)
            Debug.Log($"{midiDevice.Name} {e.Event}");
    }
}
