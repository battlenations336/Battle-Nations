
using BNR;
using ExitGames.Client.Photon;
using GameCommon;
using GameCommon.SerializedObjects;
using GameCommon.SerializedObjects.Packets;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientViews
{
    public class WorldView : MonoBehaviour
    {
        public void PurchaseEntity(string name, Vector3 position, bool flip, int uid, bool init)
        {
            BuildEntity objectToSerialize = new BuildEntity();
            objectToSerialize.Name = name;
            objectToSerialize.Uid = uid;
            objectToSerialize.PositionX = position.x;
            objectToSerialize.PositionY = position.y;
            objectToSerialize.Flip = flip;
            objectToSerialize.InitialSetup = init;
            OperationRequest request = new OperationRequest()
            {
                OperationCode = 2,
                Parameters = new Dictionary<byte, object>()
        {
          {
            PhotonEngine.instance.SubCodeParameterCode,
            (object) MessageSubCode.PurchaseEntity
          },
          {
            (byte) 5,
            MessageSerializerService.SerializeObjectOfType<BuildEntity>(objectToSerialize)
          }
        }
            };
            Debug.Log((object)string.Format("Sending Request for purchase entity {0}", (object)objectToSerialize.Name));
            PhotonEngine.instance.SendRequest(request);
        }

        public void ChangeBuildingState(
          int buildingId,
          BuildingState oldState,
          BuildingState newState,
          BuildingEntity building,
          int index)
        {
            OperationRequest request = new OperationRequest()
            {
                OperationCode = 2,
                Parameters = new Dictionary<byte, object>()
        {
          {
            PhotonEngine.instance.SubCodeParameterCode,
            (object) MessageSubCode.BuildingState
          },
          {
            (byte) 5,
            MessageSerializerService.SerializeObjectOfType<BuildingUpdate>(new BuildingUpdate()
            {
              Uid = buildingId,
              OldState = oldState,
              NewState = newState,
              Name = building.Name,
              JobName = building.JobName,
              EventCompleteTime = building.eventCompleteTime,
              Level = building.Level,
              Index = index,
              X = building.Position.x,
              Y = building.Position.y,
              Flip = building.Flip
            })
          }
        }
            };
            Debug.Log((object)"Sending Request for building update");
            PhotonEngine.instance.SendRequest(request);
        }

        public void CollectOutput(int buildingId, string worldName, string jobName)
        {
            OperationRequest request = new OperationRequest()
            {
                OperationCode = 2,
                Parameters = new Dictionary<byte, object>()
        {
          {
            PhotonEngine.instance.SubCodeParameterCode,
            (object) MessageSubCode.CollectResource
          },
          {
            (byte) 5,
            MessageSerializerService.SerializeObjectOfType<ResourceCollection>(new ResourceCollection()
            {
              Id = buildingId,
              WorldName = worldName,
              JobName = jobName
            })
          }
        }
            };
            Debug.Log((object)"Sending Request for resource collection");
            PhotonEngine.instance.SendRequest(request);
        }

        public void HurryTask(HurryTaskType type, int uid, string name)
        {
            OperationRequest request = new OperationRequest()
            {
                OperationCode = 2,
                Parameters = new Dictionary<byte, object>()
        {
          {
            PhotonEngine.instance.SubCodeParameterCode,
            (object) MessageSubCode.Hurry
          },
          {
            (byte) 5,
            MessageSerializerService.SerializeObjectOfType<GameCommon.SerializedObjects.Packets.HurryTask>(new GameCommon.SerializedObjects.Packets.HurryTask()
            {
              Uid = uid,
              Name = name,
              Type = type
            })
          }
        }
            };
            Debug.Log((object)"Sending Request for hurry task");
            PhotonEngine.instance.SendRequest(request);
        }

        public void UnitPromotion(string name, bool upgrading)
        {
            OperationRequest request = new OperationRequest()
            {
                OperationCode = 2,
                Parameters = new Dictionary<byte, object>()
        {
          {
            PhotonEngine.instance.SubCodeParameterCode,
            (object) MessageSubCode.Promote
          },
          {
            (byte) 5,
            MessageSerializerService.SerializeObjectOfType<Unit>(new Unit()
            {
              Name = name,
              Upgrading = upgrading
            })
          }
        }
            };
            Debug.Log((object)"Sending Request for promote task");
            PhotonEngine.instance.SendRequest(request);
        }

        public void UpdateMission(string _missionId, MissionState _missionState)
        {
            GameCommon.SerializedObjects.Packets.UpdateMission objectToSerialize = new GameCommon.SerializedObjects.Packets.UpdateMission();
            objectToSerialize.MissionId = _missionId;
            objectToSerialize.State = _missionState;
            if (GameData.Player.CurrentMissions.ContainsKey(_missionId) && GameData.Player.CurrentMissions[_missionId].Steps != null)
            {
                objectToSerialize.Steps = new Dictionary<string, MissionStep>();
                foreach (string key in GameData.Player.CurrentMissions[_missionId].Steps.Keys)
                {
                    MissionStep step = GameData.Player.CurrentMissions[_missionId].Steps[key];
                    objectToSerialize.Steps.Add(key, new MissionStep()
                    {
                        Complete = step.Complete,
                        Count = step.Count,
                        Goal = step.Goal,
                        Prereq = step.Prereq,
                        Seq = step.Seq
                    });
                }
            }
            OperationRequest request = new OperationRequest()
            {
                OperationCode = 2,
                Parameters = new Dictionary<byte, object>()
        {
          {
            PhotonEngine.instance.SubCodeParameterCode,
            (object) MessageSubCode.UpdateMission
          },
          {
            (byte) 5,
            MessageSerializerService.SerializeObjectOfType<GameCommon.SerializedObjects.Packets.UpdateMission>(objectToSerialize)
          }
        }
            };
            Debug.Log((object)string.Format("Sending Request for update mission {0}, {1}", (object)_missionId, (object)_missionState));
            PhotonEngine.instance.SendRequest(request);
        }

        public void UpdateEncounter(EncounterArmy encounter, string world, int award)
        {
            GameCommon.SerializedObjects.Packets.UpdateEncounter objectToSerialize = new GameCommon.SerializedObjects.Packets.UpdateEncounter();
            objectToSerialize.World = world;
            objectToSerialize.InstanceId = encounter.InstanceId;
            objectToSerialize.Name = encounter.Name;
            objectToSerialize.EventId = encounter.EventId;
            objectToSerialize.X = encounter.X;
            objectToSerialize.Y = encounter.Y;
            objectToSerialize.Type = encounter.Type;
            objectToSerialize.Status = encounter.Status;
            objectToSerialize.Created = encounter.Created;
            objectToSerialize.AwardMoney = award;
            ICollection<BNR.EncounterUnit> units = encounter.Units;
            OperationRequest request = new OperationRequest()
            {
                OperationCode = 2,
                Parameters = new Dictionary<byte, object>()
        {
          {
            PhotonEngine.instance.SubCodeParameterCode,
            (object) MessageSubCode.Encounter
          },
          {
            (byte) 5,
            MessageSerializerService.SerializeObjectOfType<GameCommon.SerializedObjects.Packets.UpdateEncounter>(objectToSerialize)
          }
        }
            };
            Debug.Log((object)string.Format("Sending Request for update encounter {0}, {1}", (object)objectToSerialize.Name, (object)objectToSerialize.Status));
            PhotonEngine.instance.SendRequest(request);
        }

        public void UpdateAchievement(string achievementId, int amount, bool complete)
        {
            GameCommon.SerializedObjects.UpdateAchievement objectToSerialize = new GameCommon.SerializedObjects.UpdateAchievement();
            objectToSerialize.AchievementId = achievementId;
            objectToSerialize.Amount = amount;
            objectToSerialize.Complete = complete;
            OperationRequest request = new OperationRequest()
            {
                OperationCode = 2,
                Parameters = new Dictionary<byte, object>()
        {
          {
            PhotonEngine.instance.SubCodeParameterCode,
            (object) MessageSubCode.Achievement
          },
          {
            (byte) 5,
            MessageSerializerService.SerializeObjectOfType<GameCommon.SerializedObjects.UpdateAchievement>(objectToSerialize)
          }
        }
            };
            Debug.Log((object)string.Format("Sending Request for update achievement {0}, {1}", (object)objectToSerialize.AchievementId, (object)objectToSerialize.Amount));
            PhotonEngine.instance.SendRequest(request);
        }
    }
}
