namespace MonochromeEngine.Utils;

public class Sprite 
{
    public string Owner { get; private set; }
    
    // string - тип анімації, List<char[,] - список анімацій
    public Dictionary<string, List<char[,]>> animation =  new Dictionary<string, List<char[,]>>();

    public Sprite(string owner)
    {
        Owner = owner;
    }

    public void AddAnimation(string name, char[,] anim)
    {
        if (!animation.ContainsKey(name))
        {
            animation.Add(name, new List<char[,]>() {anim});
        }
        else
        {
            animation[name].Add(anim);
        }
    }
}