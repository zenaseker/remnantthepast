using AmplifyShaderEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class CommandBarUI : MonoBehaviour
{
    public TMP_InputField input;
    public TextMeshProUGUI outhelp;
    public CommandBarUI()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("Create x y z:在某处召唤单位(x为单位类型，y为单位id，z为目标坐标(以'a,b'格式))");
        stringBuilder.Append("Action x y z:在时间轴上添加意图(x为单位id，y为意图序列数，z为坐标索引(可不填，以'a,b'格式))");
        stringBuilder.Append("Action_a x y z:在时间轴上添加虚拟意图(x为时间，y为强度，z为图标名)");
        stringBuilder.Append("AddTime x y:预览时点推进x(1000为1单位时点)，y为是否推进"); 
        stringBuilder.Append("FindEnemy x:获取警戒范围包含x的所有敌人(以'a,b'格式)");
        stringBuilder.Append("RecoverUnit x y:为当前单位回复，x(0为回血，其他回技力)，y为回复量");
        helptext = stringBuilder.ToString();
    }
    public void OnCommand(string command)
    {
        if (command.Contains(" "))
        {
            string[] commands = command.Split(" ");
            switch(commands[0])
            {
                case "Help":
                    Help();
                    break;
                case "Create":
                    Create(commands);
                    break;
                case "Action":
                    Action(commands);
                    break;
                case "AddTime":
                    AddTime(commands);
                    break;
                case "FindEnemy":
                    FindEnemy(commands);
                    break;
                case "RecoverUnit":
                    RecoverUnit(commands);
                    break;
                default:
                    outhelp.text = "无效的指令：" + command;
                    break;
            }

        }
        input.text = "";
    }
    readonly string helptext;
    void Help()
    {
        outhelp.text = helptext;
    }
    void Create(string[] command)
    {
        if (command.Length < 1 || (command[1] != "Char" && command[1] != "Enemy"))
        {
            outhelp.text = "指令错误：Create ?";
            return;
        }
        if (command.Length < 2)
        {
            outhelp.text = $"指令错误：Create {command[1]} ?";
            return;
        }
        if (command.Length < 3 || !command[3].Contains(","))
        {
            outhelp.text = $"指令错误：Create {command[1]} {command[2]} ?";
            return;
        }
        string[] poss = command[3].Split(",");
        Vector3 pos = new Vector3(int.Parse(poss[0]), int.Parse(poss[1]), 0);
        PoolManage.Instance.GetPoolGameObject(command[1], command[2], pos, Quaternion.identity);
    }
    void Action(string[] command)
    {
        if (command.Length < 1)
        {
            outhelp.text = "指令错误：Action ?";
            return;
        }
        if (command.Length < 2 || !int.TryParse(command[2],out int result))
        {
            outhelp.text = $"指令错误：Action {command[1]} ?";
            return;
        }
        string[] poss = command[1].Split("_");
        IRoundQueneObject robj = null;
        if (TimeManager.Instance._GameState != TimeManager.GameState.Combat)
        {
            outhelp.text = $"不处于战斗状态";
            return;
        }
        List<IRoundQueneObject> list = new List<IRoundQueneObject>(GameApp.Instance.CombatMonsterList);
        list.RemoveAll(x => ((EnemyBase)x).MonsterInfo.ID != command[1]);
        if (list.Count <= 0)
        {
            outhelp.text = $"未找到敌人：{command[1]}";
            return;
        }
        if (command.Length > 3)
        {
            string[] pos = command[3].Split(",");

            if (int.TryParse(pos[0],out int x) && int.TryParse(pos[1],out int y))
            {
                Vector3 poses = new Vector3(x, y, 0);
                robj = list.Find(x => ((MonoBehaviour)x).gameObject.transform.position == poses);
                if (robj == null)
                {
                    outhelp.text = $"未搜索到该坐标的单位：Action {command[1]} {command[3]}";
                    return;
                }
            }
            else
            {
                outhelp.text = $"{command[3]} 不是有效坐标";
                return;
            }
        }
        else
        {
            robj = list[0];
        }
        if (robj != null && ((EnemyBase)robj).ActionTree.TryGetIntent((byte)result,out MonsterIntentInfo intent))
        {
            UIManager.Instance.actionTimeLine.AddAction(new EnemyActionTime(robj, intent));
        }
    }
    void AddTime(string[] command)
    {
        if (int.TryParse(command[1], out int x))
        {
            UIManager.Instance.actionTimeLine.ShowAddTime(x / 1000f);
        }
        else
        {
            outhelp.text = $"指令错误：AddTime ？";
        }
        if (command.Length > 2 && command[2] != "0")
        {
            UIManager.Instance.actionTimeLine.TimeMove(x / 1000f);
        }
    }
    void FindEnemy(string[] command)
    {
        string[] pos = command[1].Split(",");
        if (int.TryParse(pos[0], out int x) && int.TryParse(pos[1], out int y))
        {
            Vector2Int poses = new Vector2Int(x, y);
            if (MapManager.Instance.MonsterWarningRangeObjs.Contains(new Vector2Int(x,y)))//进入敌人警戒区域
            {
                Vector2Int vectorInMap = poses - MapManager.Instance.MainmapInstance.CenterMove;
                StringBuilder stringBuilder = new StringBuilder($"坐标({x},{y})敌人有：");
                foreach (IRoundQueneObject monster in MapManager.Instance.MainmapInstance.GetGrid(vectorInMap).Monsters)
                {
                    stringBuilder.Append(((EnemyBase)monster).gameObject.name).Append(",");
                }
                outhelp.text = stringBuilder.ToString();
            }
            else
            {
                outhelp.text = $"坐标({x},{y})没有敌人";
            }
        }
        else
        {
            outhelp.text = $"{command[1]} 不是有效坐标";
        }
    }
    void RecoverUnit(string[] command)
    {
        if (GameApp.Instance._nowSelect == null)
        {
            outhelp.text = $"需要选择单位";
            return;
        }
        if (command.Length < 3)
        {
            outhelp.text = $"指令不完整";
            return;
        }
        int.TryParse(command[2], out int x);
        if (command[1] == "0")
        {
            IRoundQueneObject owner = GameApp.Instance._nowSelect.GetComponent<IRoundQueneObject>();
            if (owner.IsPlayer)
            {
                ((CharacterBase)owner).Hp.Heal(x);
            }
            else
            {
                ((EnemyBase)owner).Hp.Heal(x);
            }
        }
        else
        {
            IRoundQueneObject owner = GameApp.Instance._nowSelect.GetComponent<IRoundQueneObject>();
            if (owner.IsPlayer)
            {
                ((CharacterBase)owner).Hp.Sp(x);
            }
        }
    }
}
