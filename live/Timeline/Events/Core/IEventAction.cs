using UnityEngine;

/// <summary>
/// Her event action'ın implement etmesi gereken interface
/// </summary>
public interface IEventAction
{
    /// <summary>
    /// Action'ın benzersiz türü (örn: "ObjectVisibility", "ObjectScale")
    /// </summary>
    string ActionType { get; }

    /// <summary>
    /// Action'ı çalıştır
    /// </summary>
    /// <param name="parameters">Action parametreleri</param>
    void Execute(EventActionData actionData);

    /// <summary>
    /// Action'ı geri al (scrubbing için)
    /// </summary>
    /// <param name="parameters">Action parametreleri</param>
    void Undo(EventActionData actionData);

    /// <summary>
    /// Action'ın geçerli olup olmadığını kontrol et
    /// </summary>
    /// <param name="actionData">Kontrol edilecek data</param>
    /// <returns>True eğer action execute edilebilirse</returns>
    bool IsValid(EventActionData actionData);

    /// <summary>
    /// Action'ın sürekli güncelleme gerektirip gerektirmediği
    /// (örn: pozisyon tweening için true)
    /// </summary>
    bool RequiresContinuousUpdate { get; }
}