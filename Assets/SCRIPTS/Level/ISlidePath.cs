using UnityEngine;

namespace PaperGuy.Level
{
    public interface ISlidePath
    {
        float Length { get; }

        Vector2 GetPoint(float distance);

        Vector2 GetTangent(float distance);

        float GetClosestDistance(Vector2 worldPoint);
    }
}
