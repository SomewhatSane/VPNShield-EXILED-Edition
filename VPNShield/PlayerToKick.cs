using System;
using EXILED.Extensions;

namespace VPNShield
{
    internal enum KickReason : byte
    {
        Account,
        VPN
    }
    
    internal readonly struct PlayerToKick : IEquatable<PlayerToKick>
    {
        private readonly string userId;
        internal readonly KickReason reason;
        internal readonly uint creationTime;

        internal PlayerToKick(string userId, KickReason reason)
        {
            this.userId = userId;
            this.reason = reason;
            creationTime = (uint)EventHandlers.stopwatch.Elapsed.TotalSeconds;
        }

        public bool Equals(PlayerToKick other) =>
            string.Equals(userId, other.userId, StringComparison.InvariantCultureIgnoreCase);

        public override bool Equals(object obj) => obj is PlayerToKick other && Equals(other);

        public override int GetHashCode() =>
            userId != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(userId) : 0;
    }
}