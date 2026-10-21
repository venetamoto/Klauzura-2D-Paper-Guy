using System;
using UnityEngine;

namespace PaperGuy.Player
{
    /// <summary>Všechna „kolečka“ pohybu na jednom místě — snadno se ladí a kopírují mezi prefaby.</summary>
    [Serializable]
    public class PlayerMovementSettings
    {
        [Header("Běh")]
        public float moveSpeed = 7f;
        public float acceleration = 60f;
        public float deceleration = 70f;
        [Range(0f, 1f)] public float airControl = 0.65f;
        public float runAnimThreshold = 0.1f;

        [Header("Skok")]
        public float jumpVelocity = 13f;
        [Tooltip("Skočit jde ještě tolik sekund po sestupu z hrany.")]
        public float coyoteTime = 0.1f;
        [Tooltip("Násobitel gravitace při pádu.")]
        public float fallGravityMultiplier = 1.8f;
        [Tooltip("Násobitel gravitace, když hráč pustí skok při stoupání (krátký skok).")]
        public float lowJumpGravityMultiplier = 2.5f;
        public float maxFallSpeed = 22f;

        [Header("Kontrola země")]
        public float groundCheckRadius = 0.15f;
        public LayerMask groundLayers = 1;

        [Header("Omezení zpět")]
        [Tooltip("Odsazení od levého okraje záběru (obvykle polovina šířky collideru).")]
        public float leftEdgePadding = 0.4f;

        [Header("Klouzání (graf funkce)")]
        public float slideGravity = 25f;
        public float slideBaseAcceleration = 2f;
        public float slideMinEntrySpeed = 3f;
        public float slideMinSpeed = 2f;
        public float slideMaxSpeed = 22f;
        [Range(0f, 1.5f)] public float slideExitMomentum = 1f;
        [Range(0f, 1f)] public float slideTiltAmount = 0.6f;

        [Header("Jídlo")]
        public float eatDuration = 0.35f;
    }
}
