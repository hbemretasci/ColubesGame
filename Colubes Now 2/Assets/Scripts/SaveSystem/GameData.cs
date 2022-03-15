[System.Serializable]
public class GameData 
{
    public int firstCube;
    public int goalCube;
    public int[] tableCubes;
    
    public GameData(int first, int goal, int[] table)
    {
        firstCube = first;
        goalCube = goal;
        tableCubes = new int[6];

        for (int i = 0; i <table.Length; i++)
        {
            tableCubes[i] = table[i];
        }
    }
}
