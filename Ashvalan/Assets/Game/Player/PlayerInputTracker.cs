using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class PlayerInputTracker
{
    /*
    Animation States
    - 0 = Idle
    - 1 = Moving
    - 2 = Attack
    - 3 = Spear
    - 4 = Chain
    - 5 = Spell
    - 6 = Buff
*/

    private static List<int> playerInputCommands = new List<int>();

    public static void SubmitInput(int index)
    {
        playerInputCommands.Add(index);
    }

    public static List<int> ReadInputs(int count)
    {
        if (count > playerInputCommands.Count) return playerInputCommands;

        List<int> output = playerInputCommands.TakeLast(count).ToList();
        return output;

        
    }
}
