using Modding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BlueSR_OST
{
    public class BlueSR_OST : Mod
    {
        private readonly Assembly assembly = Assembly.GetExecutingAssembly();
        private Dictionary<string, AudioClip> blueTracks = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, string> reference = new Dictionary<string, string>()
        {
            {"S5 Green Path Bass", "bluePath"},
            {"S5 Green Path Action", "bluePath"},
            {"S5 Green Path Main", "bluePath"},
            {"S61-216 Hollow Knight", "blueVessel"},
            {"S25 Fungal Wastes MAIN", "blueWastes"},
            {"S25 Fungal Wastes BASS Mantis", "blueWastes"},
            {"S25 Fungal Wastes BASS Pizz", "blueWastes"}
        };

        internal AssetBundle blueTrackBundle = null;

        public BlueSR_OST() : base("BlueSR OST") {

            using (Stream blueStream = assembly.GetManifestResourceStream("BlueSR_OST.Resources.bluetrackbundles"))
            {
                if (blueStream != null)
                {
                    blueTrackBundle = AssetBundle.LoadFromStream(blueStream);
                    if (blueTrackBundle != null)
                    {
                        Log("Bundle made correctly");
                        AudioClip[] cliplist = blueTrackBundle.LoadAllAssets<AudioClip>();
                        foreach (AudioClip clip in cliplist)
                        {
                            blueTracks.Add(clip.name, clip);
                        }
                        Log("Done storing clips.");
                    }
                    else Log("Bundle is null");
                }
                else Log("Didn't find blueStream");
            }
            
            Log("Finished Constructing");
        }

        public override string GetVersion() => "1.2.0";

        private void Hook()
        {
            On.AudioManager.BeginApplyMusicCue += AudioSwap;
        }

        public override void Initialize()
        {
            Hook();
        }

        private IEnumerator AudioSwap(On.AudioManager.orig_BeginApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
        {
            MusicCue.MusicChannelInfo[] infos = ReflectionHelper.GetField<MusicCue, MusicCue.MusicChannelInfo[]>(musicCue, "channelInfos");
            if (infos != null)
            {
                for (int i = 0; i < infos.Length; i++)
                {
                    if (infos[i] == null) continue;
                    AudioClip origAudio = ReflectionHelper.GetField<MusicCue.MusicChannelInfo, AudioClip>(infos[i], "clip");
                    if (origAudio == null) continue;
                    if (reference.TryGetValue(origAudio.name, out string replacement) && blueTracks.TryGetValue(replacement, out AudioClip newClip))
                    {
                        ReflectionHelper.SetField<MusicCue.MusicChannelInfo, AudioClip>(infos[i], "clip", newClip);
                        Log($"Replaced '{origAudio.name}' with '{replacement}' on channel {i}");
                    }
                }
                ReflectionHelper.SetField<MusicCue, MusicCue.MusicChannelInfo[]>(musicCue, "channelInfos", infos);
            }
            yield return orig(self, musicCue, delayTime, transitionTime, applySnapshot);
        }
    }
}
