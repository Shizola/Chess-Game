using UnityEngine;

[CreateAssetMenu(fileName = "NewWorldKingAttributes", menuName = "KoK/World King Attributes")]
public class KoK_WorldKingAttributes : ScriptableObject
{
    public string kingName;        // Name of the king
    public int gold;               // Amount of gold the king possesses
    public int numberOfTroops;     // Number of troops under the king's command
    public string kingdomName;     // Name of the king's kingdom
    public BattleStyle battleStyle;      // Battle style of the king
    [Range(600, 1800)]
    public int skillElo;         // Skill of the king (600 to 1800)
    public int blunderChance;      // Chance of the king making a blunder (0 to 100)
    public bool hasAllegiance;     // Whether the king has allegiance to the player
}