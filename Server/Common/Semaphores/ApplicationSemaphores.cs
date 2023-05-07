namespace Server.Common.Semaphores;

public static class ApplicationSemaphores
{
    public static readonly SemaphoreSlim SemaphoreSlimForChangingReadyStatus;
    public static readonly SemaphoreSlim SemaphoreSlimForChangingColor;
    
    static ApplicationSemaphores()
    {
        SemaphoreSlimForChangingReadyStatus = new SemaphoreSlim(1, 1);
        SemaphoreSlimForChangingColor = new SemaphoreSlim(1, 1);
    }
}