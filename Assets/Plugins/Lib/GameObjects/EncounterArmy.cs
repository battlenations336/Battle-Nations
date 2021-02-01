
using GameCommon;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BNR
{
  public class EncounterArmy
  {
    public int InstanceId { get; set; }

    public string Name { get; set; }

    public string EventId { get; set; }

    public float X { get; set; }

    public float Y { get; set; }

    public EncounterType Type { get; set; }

    public EncounterStatus Status { get; set; }

    public DateTime Created { get; set; }

    public ICollection<EncounterUnit> Units { get; set; }

    public GameObject Marker { get; set; }
  }
}
