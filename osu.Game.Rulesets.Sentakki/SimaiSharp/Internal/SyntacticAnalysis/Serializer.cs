using System;
using System.IO;
using SimaiSharp.Structures;

namespace SimaiSharp.Internal.SyntacticAnalysis
{
    internal sealed class Serializer
    {
        private int _currentTimingChange;
        private int _currentNoteCollection;
        private double _currentTime;

        public void Serialize(MaiChart chart, StringWriter writer)
        {
            writer.Write($"({chart.TimingChanges[_currentTimingChange].tempo})");
            writer.Write($"{{{chart.TimingChanges[_currentTimingChange].subdivisions}}}");

            while (_currentTime <= chart.FinishTiming.GetValueOrDefault(0))
            {
                if (_currentTimingChange < chart.TimingChanges.Length - 1 &&
                    Math.Abs(chart.TimingChanges[_currentTimingChange + 1].time - _currentTime) < 0.01)
                {
                    _currentTimingChange++;

                    if (Math.Abs(chart.TimingChanges[_currentTimingChange].tempo -
                                 chart.TimingChanges[_currentTimingChange - 1].tempo) > double.Epsilon)
                        writer.Write($"({chart.TimingChanges[_currentTimingChange].tempo})");

                    if (Math.Abs(chart.TimingChanges[_currentTimingChange].subdivisions -
                                 chart.TimingChanges[_currentTimingChange - 1].subdivisions) > double.Epsilon)
                        writer.Write($"{{{chart.TimingChanges[_currentTimingChange].subdivisions}}}");
                }

                if (_currentNoteCollection < chart.NoteCollections.Length &&
                    Math.Abs(chart.NoteCollections[_currentNoteCollection].time - _currentTime) <=
                    0.01)
                {
                    SerializeNoteCollection(chart.NoteCollections[_currentNoteCollection], writer);

                    _currentNoteCollection++;
                }

                var timingChange = chart.TimingChanges[_currentTimingChange];
                _currentTime += timingChange.SecondsPerBeat;
                writer.Write(',');
            }
            writer.Write('E');

            writer.Flush();
            writer.Close();
        }

        private static void SerializeNoteCollection(NoteCollection notes, StringWriter writer)
        {
            char separator = notes.eachStyle == EachStyle.ForceBroken ? '`' : '/';

            if (notes.eachStyle == EachStyle.ForceEach)
                writer.Write("0/");

            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].WriteTo(writer);

                if (i != notes.Count - 1)
                    writer.Write(separator);
            }
        }
    }
}
