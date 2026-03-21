namespace _Project.Scripts.Sense
{
    [System.Serializable]
    public class SenseData
    {
        public SenseType type;
        public float level;
    }
    
    public enum SenseType
    {
        Hearing,
        Smell,
        VisionR,
        VisionB,
        VisionG,
    }
}