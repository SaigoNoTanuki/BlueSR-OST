using HutongGames;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static UnityEngine.UI.Selectable;

namespace BlueSR_OST
{
    public class BlueSR_OST : Mod, ITogglableMod
    {
        private AudioClip customGreenPath;
        private AudioClip customWastes;
        private AudioClip customVessel;
        public BlueSR_OST() : base("BlueSR OST") { }
        public override string GetVersion() => "1.0.0";
        private On.AudioManager.hook_ApplyMusicCue _applyMusicCue;

        public override void Initialize()
        {
            string modDirectory = Path.GetDirectoryName(typeof(BlueSR_OST).Assembly.Location);
            string bluePath = Path.Combine(modDirectory, "bluePath.wav");
            string blueWastes = Path.Combine(modDirectory, "blueWastes.wav");
            string blueVessel = Path.Combine(modDirectory, "blueVessel.wav");

            byte[] bytesGP = File.ReadAllBytes(bluePath);
            customGreenPath = WavUtility.ToAudioClip(bytesGP);
            byte[] bytesFW = File.ReadAllBytes(blueWastes);
            customWastes = WavUtility.ToAudioClip(bytesFW);
            byte[] bytesSV = File.ReadAllBytes(blueVessel);
            customVessel = WavUtility.ToAudioClip(bytesSV);

            _applyMusicCue = (orig, self, cue, delay, transition, fadeIn) =>
            {
                if (cue?.name == "Greenpath" && customGreenPath != null)
                {
                    cue = ScriptableObject.Instantiate(cue);
                    AudioSwap(cue, customGreenPath);
                    Log("Playing: bluePath");
                }
                else if(cue?.name == "Fungus" && customWastes != null)
                {
                    cue = ScriptableObject.Instantiate(cue);
                    AudioSwap(cue, customWastes);
                    Log("Playing: blueWastes");
                }
                else if(cue?.name == "HKBattle" && customVessel != null)
                {
                    cue = ScriptableObject.Instantiate(cue);
                    AudioSwap(cue, customVessel);
                    Log("Playing: blueVessel");
                }
                orig(self, cue, delay, transition, fadeIn);
            };
            On.AudioManager.ApplyMusicCue += _applyMusicCue;
        }
        private void AudioSwap(MusicCue cue, AudioClip customPath)
        {
            var channelInfosField = typeof(MusicCue).GetField("channelInfos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var channelInfos = channelInfosField.GetValue(cue) as MusicCue.MusicChannelInfo[];
            var clipField = typeof(MusicCue.MusicChannelInfo).GetField("clip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (var channel in channelInfos)
            {
            clipField.SetValue(channel, customPath);
            }
        }
        public void Unload()
        {
            On.AudioManager.ApplyMusicCue -= _applyMusicCue;
        }
    }
}