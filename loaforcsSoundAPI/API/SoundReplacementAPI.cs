using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.API {
    public static class SoundReplacementAPI {
        internal static Dictionary<string, AudioFileFormat> FileFormats = new Dictionary<string, AudioFileFormat>();

        public static void RegisterAudioFormat(string extension, AudioFileFormat format) {
            FileFormats.Add(extension, format);
        }
    }
}
