﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using StreamCore.Chat;
using StreamCore;
using StreamCore.Config;
using StreamCore.Utils;
using StreamCore.YouTube;
using StreamCore.Twitch;
using static EmoteRain.Logger;


namespace EmoteRain
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.<br/>
    /// For a full list of Messages a Monobehaviour can receive from the game,<br/>see <seealso cref="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html"/>.
    /// </summary>
    internal class TwitchMSGHandler : ITwitchIntegration
    {
        public bool IsPluginReady { get; set; }

        public TwitchMSGHandler()
        {
            TwitchMessageHandlers.PRIVMSG += MSGHandler;
            IsPluginReady = true;
        }

        private static void MSGHandler(TwitchMessage twitchMsg)
        {
            Log("Got Twitch Msg!\nrawMessage: " + twitchMsg.rawMessage);
            string et = getEmoteTagFromMsg(twitchMsg.rawMessage);
            Log("EmoteTag: " + et);
            string[] eids = arraify(et);
            if (eids.Length > 0)
            {
                Log("EmoteIDs:");
                foreach (string e in eids)
                {
                    Log("ID {" + e + "} has link to https://static-cdn.jtvnw.net/emoticons/v1/" + e + "/2.0");
                }
                Log("Sending to Emote-Queue...");
                queueEmoteSprites(eids);
            }
            else
            {
                Log("No Emotes in msg to queue!");
            }
        }

        private static void queueEmoteSprites(string[] emoteID)
        {
            foreach (string e in emoteID)
            {
                Log("Trying to enqueue EmoteID " + e);

                HMMainThreadDispatcher.instance.Enqueue(EnqueueEmote(e));
                Log("Enqueued EmoteID: " + e);
            }
        }

        private static IEnumerator EnqueueEmote(string e){
            yield return null;
            RequestCoordinator.EmoteQueue(e);
        }

        private static string getEmoteTagFromMsg(string msg)
        {
            //  bspMsg for msg where "moepHi" is an emote
            //      MessageTest2 Emote: moepHi
            // 
            //  rawMsg is 
            //      @badge-info=;badges=broadcaster/1,premium/1;color=#000F92;display-name=Cr4sher_;emotes=301242724:20-25;flags=;id=4fe7c5d4-a9f4-4190-8d7d-6ff55d75ab80;mod=0;room-id=133537093;
            //      subscriber=0;tmi-sent-ts=1582742897726;turbo=0;user-id=133537093;user-type= :cr4sher_!cr4sher_@cr4sher_.tmi.twitch.tv PRIVMSG #cr4sher_ :MessageTest2 Emote: moepHi
            //
            //  therefore "emotes=301242724:20-25" should be extracted

            string[] tags = msg.Split(';');
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i].StartsWith("emotes="))
                {
                    return tags[i];
                }
            }
            return "emotes=";
        }

        private static string[] arraify(string emoteIDs)
        {
            //  bspInput
            //      emotes=301290052:29-35/301242724:37-42/115845:44-52/106293:54-60
            //  
            //  bspOutput
            //      {"301290052","301242724","115845","106293"}

            List<string> emoteIDArray = new List<string>();

            if (emoteIDs.Split('=').Length < 2) return emoteIDArray.ToArray();
            string[] emotesWithIndex = emoteIDs.Split('=')[1].Split('/');
            foreach (string e in emotesWithIndex)
            {
                string currentEmoteID = e.Split(':')[0];
                string[] inLine = e.Split(':')[1].Split(',');
                foreach (var _ in inLine)
                {
                    emoteIDArray.Add(currentEmoteID);
                }
            }
            return emoteIDArray.ToArray();
        }
    }
}