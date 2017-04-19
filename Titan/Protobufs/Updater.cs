﻿using System;
using System.IO;
using System.Net;
using EasyHttp.Http;
using log4net;
using Titan.Json;

namespace Titan.Protobufs
{
    public class Updater
    {

        public static readonly ILog Log = LogManager.GetLogger(typeof(Updater));

        public static string Base = "https://raw.githubusercontent.com/SteamRE/SteamKit/master/Resources/Protobufs/csgo/";

        public static DirectoryInfo ProtoDir = new DirectoryInfo(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "protobufs");
        public static FileInfo HistoryFile = new FileInfo(Path.Combine(ProtoDir.ToString(), "history.sha"));

        public static string[] Protos =
        {
            "base_gcmessages.proto",
            "steammessages.proto",
            "cstrike15_gcmessages.proto",
            "gcsdk_gcmessages.proto",
            "engine_gcmessages.proto"
        };

        public static void Update()
        {
            if(!ProtoDir.Exists)
            {
                Log.Debug("Protobuf directory doesn't exist. Creating it...");
                ProtoDir.Create();
            }

            foreach(var proto in Protos)
            {
                var file = new FileInfo(proto);
                if(file.Exists)
                {
                    file.Delete();
                }

                using(var webClient = new WebClient())
                {
                    Log.DebugFormat("Downloading \"{0}\" Protobuf file.", proto);
                    webClient.DownloadFile(new Uri(Base + proto), Path.Combine(ProtoDir.ToString(), proto));
                }
            }

            Log.Debug("Saving hash for update checking.");
            SaveHash();

            Log.Info("Successfully updated Protobufs.");
        }

        public static bool RequiresUpdate()
        {
            Log.Debug("Sending Request to GitHub.");

            var client = new HttpClient();
            client.Request.Accept = "application/vnd.github.v3+json";
            client.Request.UserAgent = "https://github.com/Marc3842h/Titan-Report";
            var response = client.Get("https://api.github.com/repos/SteamRE/SteamKit/contents/Resources/Protobufs/csgo/engine_gcmessages.proto");

            var result = response.StaticBody<GitHubResponse>();

            Log.DebugFormat("Successfully awaited. SHA Result: {0}", result.Sha);

            if(HistoryFile.Exists)
            {
                var content = File.ReadAllLines(HistoryFile.FullName);

                if(content.Length >= 1)
                {
                    var sha = content[0];

                    if(sha == result.Sha)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void SaveHash()
        {
            if(HistoryFile.Exists)
            {
                HistoryFile.Delete();
            }

            var client = new HttpClient();
            client.Request.Accept = "application/vnd.github.v3+json";
            client.Request.UserAgent = "https://github.com/Marc3842h/Titan-Report";
            var response = client.Get("https://api.github.com/repos/SteamRE/SteamKit/contents/Resources/Protobufs/csgo/engine_gcmessages.proto");

            var result = response.StaticBody<GitHubResponse>();

            File.WriteAllText(HistoryFile.FullName, result.Sha);
        }

    }
}