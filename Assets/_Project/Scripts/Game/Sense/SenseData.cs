namespace _Project.Scripts.Sense
{
    // Explicit IDs match the rabbit types serialized in scenes. Do not reorder them.
    public enum RabbitType
    {
        Vision = 0,
        Hearing = 1,
        Smell = 2,
    }

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