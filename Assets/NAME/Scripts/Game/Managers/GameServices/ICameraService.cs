namespace Unity.NAME.Game
{
    public interface ICameraService : IGameService
    {
        void RegisterCamera<T>(T camera) where T : BaseCamera;
        void UnregisterCamera<T>(T camera) where T : BaseCamera;
        T GetCamera<T>() where T : BaseCamera;
    }
}
