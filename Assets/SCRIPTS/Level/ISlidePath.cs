using UnityEngine;

namespace PaperGuy.Level
{
    /// <summary>
    /// Trajektorie, po které hráč klouže. Je parametrizovaná délkou oblouku,
    /// takže rychlost podél ní je rovnoměrná bez ohledu na tvar grafu.
    /// Implementace: <see cref="FunctionGraphPath"/> (matematický graf);
    /// případně obal nad SplineContainer / SpriteShape.
    /// </summary>
    public interface ISlidePath
    {
        float Length { get; }

        /// <summary>Světový bod ve vzdálenosti <paramref name="distance"/> od začátku.</summary>
        Vector2 GetPoint(float distance);

        /// <summary>Normalizovaná tečna ve směru pohybu.</summary>
        Vector2 GetTangent(float distance);

        /// <summary>Vzdálenost podél dráhy k pozici nejbližší danému bodu.</summary>
        float GetClosestDistance(Vector2 worldPoint);
    }
}
