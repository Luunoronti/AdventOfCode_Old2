
namespace Year2016;



// map

// X*Y character map
// with information on blockers, if any

// traveller will have options to:
// walk on map
// if at the end of map, or blocking char,
// it will call a function to establish what to do
// but we may also introduce default behaviour,
// like "StayInPlace", "TurnLeft", "TurnRight", "GoBack", etc

class Map<TType>
{
    private int sizeX;
    private int sizeY;
    private List<TType> mapActual = [];

    public Map(int sizeX, int sizeY)
    {
        SizeX = sizeX;
        SizeY = sizeY;
    }
    public Map(int sizeX, int sizeY, TType defaultFill)
    {
        DefaultFill = defaultFill;
        SizeX = sizeX;
        SizeY = sizeY;
    }
    public Map(int sizeX, int sizeY, Func<int, TType> fillFunc)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        for (int i = 0; i < sizeX * sizeY; i++)
        {
            mapActual[i] = fillFunc(i);
        }
    }
    public Map(int sizeX, int sizeY, Func<int, int, TType> fillFunc)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        for (int y = 0; y < sizeY; y++)
        {
            for (int i = 0; i < sizeX; i++)
            {
                mapActual[i + (y * sizeX)] = fillFunc(i, y);
            }
        }
    }


    public TType DefaultFill { get; set; } = default;
    public int SizeX
    {
        get => sizeX;
        set
        {
            sizeX = value;
            UpdateMap();

        }
    }
    public int SizeY
    {
        get => sizeY;
        set
        {
            sizeY = value;
            UpdateMap();

        }
    }

    private void UpdateMap()
    {
        if (SizeX <= 0 || SizeY <= 0) return;
        //TODO: we will have to copy the map
        // but for now, just fill it with defaults
        mapActual = Enumerable.Repeat(DefaultFill, SizeX * SizeX).ToList();
    }
}

class Day02
{
    public string Part1(PartInput Input)
    {
        //var map = new Map<int> { DefaultFill = 0, SizeX = 3, SizeY = 3 };


        long response = Input.LineWidth;
        return response.ToString();
    }
    public string Part2(PartInput Input)
    {
        long response = Input.LineWidth;
        return response.ToString();
    }
}
