using BNR;
using ExitGames.Client.Photon;
using GameCommon;
using GameCommon.SerializedObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientViews
{
    public class BattleView : MonoBehaviour
    {
        public void JoinBattleQueue(GridLayoutCtrl_Base _grid)
        {
            int cellCount = _grid.CellCount();
            GridUnit[] gridLayout = new GridUnit[cellCount];
            for (int x = 0; x < (cellCount - 1); x++)
            {
                gridLayout[x] = new GridUnit();
                if (_grid.Cells[x].Active && _grid.Cells[x].Unit != null && _grid.Cells[x].Unit.Name != null && _grid.Cells[x].Unit.Name != string.Empty)
                {
                    gridLayout[x].Unit = _grid.Cells[x].Unit.Name;
                    gridLayout[x].Health = 0;
                }
            }
            OperationRequest request = new OperationRequest() { OperationCode = (byte)MessageOperationCode.Battle,
                Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, MessageSubCode.JoinQueue },
                                { (byte)MessageParameterCode.Object, MessageSerializerService.SerializeObjectOfType(gridLayout)},
}
            };
            Debug.Log("Sending Request for join battle queue");
            PhotonEngine.instance.SendRequest(request);
        }

        public void Surrender()
        {
            OperationRequest request = new OperationRequest()
            {
                OperationCode = (byte)MessageOperationCode.Battle,
                Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, MessageSubCode.Surrender },
}
            };
            Debug.Log("Sending Request for surrendering");
            PhotonEngine.instance.SendRequest(request);
        }

        public void Pass()
        {
            OperationRequest request = new OperationRequest()
            {
                OperationCode = (byte)MessageOperationCode.Battle,
                Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, MessageSubCode.Pass },
}
            };
            Debug.Log("Sending Request for passing");
            PhotonEngine.instance.SendRequest(request);
        }

        public void LeaveBattleQueue()
        {
            OperationRequest request = new OperationRequest()
            {
                OperationCode = (byte)MessageOperationCode.Battle,
                Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, MessageSubCode.LeaveQueue },
}
            };
            Debug.Log("Sending Request for leave battle queue");
            PhotonEngine.instance.SendRequest(request);
        }

        public void LaunchAttack(GridLayoutCtrl_Base playerGrid, GridLayoutCtrl_Base enemyGrid)
        {
            int cellCount = playerGrid.CellCount();

            AttackDamage attackDamage = new AttackDamage();
            attackDamage.Source = playerGrid.AttackingCell;
            attackDamage.Target = enemyGrid.TargetCell;
            attackDamage.BaseDamage = playerGrid.AttackDamageBase;
            attackDamage.Damage = new UnitDamage[playerGrid.Cells.Length];
            attackDamage.UnitName = playerGrid.Attacker().Unit.Name;
            for (int x = 0; x <= (cellCount - 1); x++)
            {
                if (enemyGrid.Cells[x].UnitAlive())
                {
                    attackDamage.Damage[x] = new UnitDamage();
                    attackDamage.Damage[x].Type = DamageType.None;
//                    if (enemyGrid.Cells[x].LightUp == HighlightType.Damage)
//                        attackDamage.Damage[x].Type = DamageType.Direct;
//                    if (enemyGrid.Cells[x].LightUp == HighlightType.SplashDamage)
//                        attackDamage.Damage[x].Type = DamageType.Splash;
                }
            }

            foreach (int cell in enemyGrid.TargetList)
            {
                if (enemyGrid.Cells[cell].UnitAlive())
                {
                    attackDamage.Damage[cell].Type = DamageType.Direct;
                }
            }

            foreach (int cell in enemyGrid.SplashList)
            {
                if (enemyGrid.Cells[cell].UnitAlive())
                {
                    attackDamage.Damage[cell].Type = DamageType.Splash;
                }
            }

            if (enemyGrid.Cells[enemyGrid.TargetCell].UnitAlive())
            {
                attackDamage.Damage[enemyGrid.TargetCell].Type = DamageType.Splash;
            }

            OperationRequest request = new OperationRequest()
            {
                 
                OperationCode = (byte)MessageOperationCode.Battle,
                Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, MessageSubCode.Attack },
                { (byte)MessageParameterCode.Object, MessageSerializerService.SerializeObjectOfType(attackDamage)},
}
            };
            PhotonEngine.instance.SendRequest(request);
        }

        public void LaunchAttack1(BattleGrid playerGrid, GridLayoutCtrl enemyGrid)
        {
            AttackDamage attackDamage = new AttackDamage();
            attackDamage.Source = playerGrid.AttackingCell;
            attackDamage.Target = enemyGrid.TargetCell;
            attackDamage.BaseDamage = playerGrid.AttackDamage;
            attackDamage.Damage = new UnitDamage[playerGrid.Cells.Length];
            for (int x = 0; x <= 12; x++)
            {
                if (enemyGrid.Cells[x].UnitAlive())
                {
                    attackDamage.Damage[x] = new UnitDamage();
                    attackDamage.Damage[x].Type = DamageType.None;
                    if (enemyGrid.Cells[x].LightUp == HighlightType.Damage)
                        attackDamage.Damage[x].Type = DamageType.Direct;
                    if (enemyGrid.Cells[x].LightUp == HighlightType.SplashDamage)
                        attackDamage.Damage[x].Type = DamageType.Splash;
                }
            }
            OperationRequest request = new OperationRequest()
            {

                OperationCode = (byte)MessageOperationCode.Battle,
                Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, MessageSubCode.Attack },
                { (byte)MessageParameterCode.Object, MessageSerializerService.SerializeObjectOfType(attackDamage)},
}
            };
            Debug.Log("Sending Request for attack");
            PhotonEngine.instance.SendRequest(request);
        }

    }
}
