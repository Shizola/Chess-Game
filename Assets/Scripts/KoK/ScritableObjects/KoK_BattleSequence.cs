using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBattleSequence", menuName = "KoK/Battle Sequence")]
public class KoK_BattleSequence : ScriptableObject
{    
    public KoK_PieceData_King king;
    public KoK_Battle[] battles;
}

[System.Serializable]
public class KoK_Battle
{
    public string startingFenPosition;
    public bool hideTheKingMode;
}