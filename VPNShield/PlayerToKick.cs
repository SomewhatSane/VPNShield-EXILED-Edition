using System;

namespace VPNShield
{
    public enum KickReason : byte
    {
        None,
        AccountAge,
        AccountPlaytime,
        AccountPrivate,
        AccountSteamError,
        VPN
    }

    internal readonly struct PlayerToKick : IEquatable<PlayerToKick>
    {
        internal readonly string userId;
        internal readonly KickReason reason;
        internal readonly uint creationTime;

        internal PlayerToKick(string userId, KickReason reason)
        {
            this.userId = userId;
            this.reason = reason;
            creationTime = (uint)EventHandlers.stopwatch.Elapsed.TotalSeconds;
        }

        public bool Equals(PlayerToKick other)
        {
            return string.Equals(userId, other.userId, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerToKick other && Equals(other);
        }

        public override int GetHashCode()
        {
            return userId != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(userId) : 0;
        }
    }
}