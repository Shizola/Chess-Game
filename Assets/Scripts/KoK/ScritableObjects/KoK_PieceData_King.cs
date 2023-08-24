using UnityEngine;

[CreateAssetMenu(fileName = "NewPieceData-King", menuName = "KoK/Piece Data - King")]
public class KoK_PieceData_King : KoK_PieceData
{
    [Range(600, 2600)]
    public int baseSkillElo;         // Skill of the king (600 to 1800)
    public int blunderChance;      // Chance of the king making a blunder (0 to 100)
    public BattleStyle battleStyle;      // Battle style of the king
    public bool hasAllegiance;     // Whether the king has allegiance to the player
}