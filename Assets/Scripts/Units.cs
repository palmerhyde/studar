using UnityEngine;

namespace Studar
{
    /// <summary>
    /// LDraw &lt;-&gt; Unity unit and coordinate conversion. Per the spec (§4), this is the one
    /// place the conversion lives — bake it here once and never re-derive it elsewhere.
    ///
    /// LDraw:  unit = LDU, 1 LDU = 0.4 mm, -Y is up (Y points down), right-handed.
    /// Unity:  unit = metre,                +Y is up,                left-handed.
    ///
    /// Rule (LDraw -> Unity): scale positions by 0.0004 (LDU -> m) and negate Y to fix up-axis.
    /// A wrong scale or an un-flipped Y is the classic day-one bug, so verify imported geometry
    /// is upright and correctly sized before doing anything else.
    /// </summary>
    public static class Units
    {
        // --- Scale ---
        /// <summary>1 LDU in metres (0.4 mm).</summary>
        public const float LduToMeter = 0.0004f;

        // --- Key LEGO dimensions, in LDU (memorised from spec §4) ---
        /// <summary>Stud-to-stud pitch: the horizontal grid everything snaps to.</summary>
        public const float StudPitchLdu = 20f;   // 8 mm
        /// <summary>Height of one brick (= 3 plates).</summary>
        public const float BrickHeightLdu = 24f;  // 9.6 mm
        /// <summary>Height of one plate.</summary>
        public const float PlateHeightLdu = 8f;   // 3.2 mm

        // --- Same dimensions, pre-converted to metres (convenience) ---
        public const float StudPitchMeters = StudPitchLdu * LduToMeter;    // 0.008 m
        public const float BrickHeightMeters = BrickHeightLdu * LduToMeter; // 0.0096 m
        public const float PlateHeightMeters = PlateHeightLdu * LduToMeter; // 0.0032 m

        /// <summary>Convert a scalar length from LDU to metres.</summary>
        public static float LduToMeters(float ldu) => ldu * LduToMeter;

        /// <summary>
        /// Convert an LDraw position (LDU, -Y up) to a Unity position (metres, +Y up):
        /// negate Y for the up-axis flip, then scale to metres.
        /// </summary>
        public static Vector3 LdrawToUnity(Vector3 ldraw)
            => new Vector3(ldraw.x, -ldraw.y, ldraw.z) * LduToMeter;

        /// <summary>Inverse of <see cref="LdrawToUnity"/>: Unity metres -> LDraw LDU.</summary>
        public static Vector3 UnityToLdraw(Vector3 unity)
        {
            Vector3 ldu = unity / LduToMeter;
            return new Vector3(ldu.x, -ldu.y, ldu.z);
        }
    }
}
