﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitlesParser.Classes.Utils;

namespace SubtitlesParser.Classes.Writers
{
    /// <summary>
    /// A writer for the Substation Alpha subtitles format.
    /// See http://en.wikipedia.org/wiki/SubStation_Alpha for complete explanations.
    /// Example output:
    /// [Script Info]
    /// ; Script generated by SubtitlesParser v1.4.9.0
    /// ; https://github.com/AlexPoint/SubtitlesParser
    /// ScriptType: v4.00
    /// WrapStyle: 3
    ///
    /// [Events]
    /// Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
    /// Dialogue: 0,0:18:03.87,0:18:04.23,,,0,0,0,,Oh?
    /// Dialogue: 0,0:18:05.19,0:18:05.90,,,0,0,0,,What was that?
    /// </summary>
    public class SsaWriter : ISubtitlesWriter
    {
        /// <summary>
        /// Write the SSA file header to a text writer 
        /// </summary>
        /// <param name="writer">The TextWriter to write to</param>
        /// <param name="wrapStyle">The <see cref="SsaWrapStyle">wrap style</see> the player should use</param>
        private void WriteHeader(TextWriter writer, SsaWrapStyle wrapStyle = SsaWrapStyle.None)
        {
            writer.WriteLine(SsaFormatConstants.SCRIPT_INFO_LINE);
            writer.WriteLine($"{SsaFormatConstants.COMMENT} Script generated by SubtitlesParser v{GetType().Assembly.GetName().Version}");
            writer.WriteLine($"{SsaFormatConstants.COMMENT} https://github.com/AlexPoint/SubtitlesParser");
            writer.WriteLine("ScriptType: v4.00"); // the SSA format 
            writer.WriteLine($"{SsaFormatConstants.WRAP_STYLE_PREFIX}{wrapStyle}");
            writer.WriteLine(); // blank line between sections

            writer.Flush();
        }

        /// <summary>
        /// Asynchronously write the SSA file header to a text writer 
        /// </summary>
        /// <param name="writer">The TextWriter to write to</param>
        /// <param name="wrapStyle">The <see cref="SsaWrapStyle">wrap style</see> the player should use</param>
        private async Task WriteHeaderAsync(TextWriter writer, SsaWrapStyle wrapStyle = SsaWrapStyle.None)
        {
            await writer.WriteLineAsync(SsaFormatConstants.SCRIPT_INFO_LINE);
            await writer.WriteLineAsync($"{SsaFormatConstants.COMMENT} Script generated by SubtitlesParser v{GetType().Assembly.GetName().Version}");
            await writer.WriteLineAsync($"{SsaFormatConstants.COMMENT} https://github.com/AlexPoint/SubtitlesParser");
            await writer.WriteLineAsync("ScriptType: v4.00"); // the SSA format 
            await writer.WriteLineAsync($"{SsaFormatConstants.WRAP_STYLE_PREFIX}{wrapStyle}");
            await writer.WriteLineAsync(); // blank line between sections

            await writer.FlushAsync();
        }

        /// <summary>
        /// Converts a subtitle item into an SSA formatted dialogue line
        /// </summary>
        /// <param name="subtitleItem">The SubtitleItem to convert</param>
        /// <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
        /// <returns>The full dialogue line</returns>
        private string SubtitleItemToDialogueLine(SubtitleItem subtitleItem, bool includeFormatting)
        {
            string[] fields = new string[10]; // style, name, and effect fields are left blank
            fields[0] = "0"; // layer
            fields[1] = TimeSpan.FromMilliseconds(subtitleItem.StartTime).ToString(@"h\:mm\:ss\.fff"); // start
            fields[2] = TimeSpan.FromMilliseconds(subtitleItem.EndTime).ToString(@"h\:mm\:ss\.fff"); // end
            fields[5] = "0"; // left margin
            fields[6] = "0"; // right margin 
            fields[7] = "0"; // vertical margin

            // combine all items in the `Lines` property into a single string, with each item being seperated by an SSA newline (\N)
            // check if we should be including formatting or not (default to use formatting if plaintextlines isn't set) 
            List<string> lines = includeFormatting == false && subtitleItem.PlaintextLines != null ?
                subtitleItem.PlaintextLines:
                subtitleItem.Lines;
            fields[9] = lines.Aggregate(string.Empty, (current, line) => current + $"{line}\\N").TrimEnd('\\', 'N');


            return SsaFormatConstants.DIALOGUE_PREFIX + string.Join(SsaFormatConstants.SEPARATOR.ToString(), fields);
        }

        /// <summary>
        /// Write a list of subtitle items to a stream in the SSA/ASS format synchronously
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="subtitleItems">The subtitle items to write</param>
        /// <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
        public void WriteStream(Stream stream, IEnumerable<SubtitleItem> subtitleItems, bool includeFormatting = true)
        {
            using (TextWriter writer = new StreamWriter(stream))
            {
                WriteHeader(writer);

                writer.WriteLine(SsaFormatConstants.EVENT_LINE);
                writer.WriteLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text"); // column headers
                foreach (SubtitleItem item in subtitleItems)
                    writer.WriteLine(SubtitleItemToDialogueLine(item, includeFormatting));
            }
        }

        /// <summary>
        /// Write a list of subtitle items to a stream in the SSA/ASS format asynchronously
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="subtitleItems">The subtitle items to write</param>
        /// <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
        public async Task WriteStreamAsync(Stream stream, IEnumerable<SubtitleItem> subtitleItems, bool includeFormatting = true)
        {
            using (TextWriter writer = new StreamWriter(stream))
            {
                await WriteHeaderAsync(writer);

                await writer.WriteLineAsync(SsaFormatConstants.EVENT_LINE);
                await writer.WriteLineAsync("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text"); // column headers
                foreach (SubtitleItem item in subtitleItems)
                    await writer.WriteLineAsync(SubtitleItemToDialogueLine(item, includeFormatting));
            }
        }
    }
}
