using loaforcsSoundAPI.API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace loaforcsSoundAPI.Providers.Formats
{
    internal class Mp3AudioFormat : AudioFormatProvider
    {
        public override AudioClip LoadAudioClip(string path)
        {
            return LoadFromUWR(path, AudioType.AUDIOQUEUE);
        }
    }
}
