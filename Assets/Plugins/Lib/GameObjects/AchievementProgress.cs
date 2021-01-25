using GameCommon.SerializedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNR
{
    public class AchievementProgress
    {
        public int Count { get; set; }
        public bool Complete { get; set; }
        public DateTime CompleteDate { get; set; }

        public AchievementProgress(Achievement achievement)  //int count, bool complete, DateTime completeDate)
        {
            Count = achievement.Count;
            Complete = achievement.Complete;
            CompleteDate = achievement.CompleteDate;
        }
    }
}
