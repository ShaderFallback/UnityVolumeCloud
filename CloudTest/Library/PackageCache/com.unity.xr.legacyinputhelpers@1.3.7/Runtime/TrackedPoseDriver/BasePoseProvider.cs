using System;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.XR.Interaction
{
    /// <summary>
    /// The BasePoseProvider type is used as the base interface for all "Pose Providers"
    /// Implementing this abstract class will allow the Pose Provider to be linked to a Tracked Pose Driver.
    /// </summary>
    [Serializable]
    public abstract class BasePoseProvider : MonoBehaviour
    {
        /// <summary> Gets the Pose value from the implementor of this abstract class. returns true on success, false on error.</summary>
        public abstract bool TryGetPoseFromProvider(out Pose output);
    }
}
