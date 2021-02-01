
using BNR;
using GameCommon.SerializedObjects;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.ClientHandlers
{
    public class ProfileHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(
          Dictionary<byte, object> parameters,
          string debugMessage,
          int returnCode)
        {
            if (returnCode == 0)
            {
                Profile profile = this.DecompressAndDeserialize<Profile>((byte[])parameters[(byte)5]);
                if (profile == null || GameData.GameLoaded)
                    return;
                GameData.LoadGameData();
                GameData.InitSettings();
                GameData.InitPlayer(GameData.Player.Name, profile);
                MapConfig.InitHome();
                if (GameData.Player.Intro)
                    SceneManager.LoadScene("PlayerMap");
                else
                    SceneManager.LoadScene("Video");
                Debug.LogFormat("Profile - {0}", parameters[(byte)5]);
            }
            else
                Debug.LogFormat("Profile error {0}", (object)debugMessage);
        }

        public T DecompressAndDeserialize<T>(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (GZipStream gzipStream = new GZipStream((Stream)memoryStream, CompressionMode.Decompress, true))
                    return (T)new BinaryFormatter().Deserialize((Stream)gzipStream);
            }
        }
    }
}
