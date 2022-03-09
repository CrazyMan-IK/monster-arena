using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonsterArena.Models
{
    public class ScriptableObjectWithID : ScriptableObject, ISerializationCallbackReceiver
    {
        private static readonly Dictionary<ScriptableObjectWithID, string> _objectToString =
            new Dictionary<ScriptableObjectWithID, string>();

        private static readonly Dictionary<string, ScriptableObjectWithID> _stringToObject =
            new Dictionary<string, ScriptableObjectWithID>();

        [SerializeField]
        private string _internalId;

        /*[SerializeField]
        private long _createdAtTicks;*/

        [NonSerialized]
        private bool _internalIdWasUpdated;

        // The only attribute we want to provide externally is the InternalID value.
        // The remainder is for in-editor tracking, and scriptable object asset save/loading.
        public string InternalID => _internalId;

/*#if UNITY_EDITOR
        private string CreatedAt => new DateTime(_createdAtTicks).ToString(CultureInfo.CurrentCulture);
#endif*/

        protected void OnEnable()
        {
            ProcessRegistration(this);

            // If we updated the internalId during serialization, save the asset.
            if (!_internalIdWasUpdated)
            {
                return;
            }

            _internalIdWasUpdated = false;

#if UNITY_EDITOR
            // Before/After Serialize methods cannot mark dirty.
            //   Without this, the change we made in registration is not saved.
            //   If something else changed on the asset, such as the Display Name
            //   then that will cause it to be marked dirty, and saved.
            //   But, extensive testing has shown that there are cases where
            //   only the UUID is updated and never saved.
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        protected void OnDestroy()
        {
            Debug.LogWarning($"Unexpected object destroyed. {_internalId}");
            UnregisterObject(this);
            _internalId = null;
        }

        public void OnAfterDeserialize()
        {
            ProcessRegistration(this);
        }

        public void OnBeforeSerialize()
        {
            ProcessRegistration(this);
        }

        private static void ProcessRegistration(ScriptableObjectWithID obj)
        {
            // See if we already know about this object.
            if (_objectToString.TryGetValue(obj, out var existingId))
            {
                if (obj._internalId != existingId)
                {
                    // Logging an error since this will change this object's ID.
                    Debug.LogError($"Inconsistency: {obj.name} {obj._internalId} / {existingId}");
                    obj._internalId = existingId;
                }

                // Found object instance, ensure StringToObject contains.
                if (_stringToObject.ContainsKey(existingId))
                {
                    return;
                }

                // DB inconsistency
                Debug.LogWarning("Inconsistent database tracking.");
                _stringToObject.Add(existingId, obj);

                return;
            }

            // See if this object's Internal ID is empty. Easy case, create.
            if (string.IsNullOrEmpty(obj._internalId))
            {
                GenerateInternalId(obj);

                RegisterObject(obj);
                return;
            }

            // Ensure we don't already have the Internal ID registered.
            // If not, then we don't know about the object, nor the Internal ID, so just register.
            if (!_stringToObject.TryGetValue(obj._internalId, out var knownObject))
            {
                // ID not known to the DB, so just register it
                RegisterObject(obj);
                return;
            }

            // We DO know about the Internal ID, and it matches this object. Weird... just register it.
            if (knownObject == obj)
            {
                // DB inconsistency
                Debug.LogWarning("Inconsistent database tracking.");
                _objectToString.Add(obj, obj._internalId);
                return;
            }

            // We know about the Internal ID, but it isn't tied to any object. This object claims to
            // be that Internal ID.... okay, register it.
            if (knownObject == null)
            {
                // Object in DB got destroyed, replace with current object.
                Debug.LogWarning("Unexpected registration problem.");
                RegisterObject(obj, true);
                return;
            }

            // Otherwise:
            // 1) Object database did NOT contain this object.
            // 2) We did find a different object with the SAME identifier.
            // Thus, we have a duplicate.
            //
            // Through extensive testing, it appears the duplicated item will be updated.
            // The original item will not have its hash updated. Save games referencing that
            // hash should remain functional.
            //
            // Designers should never repurpose a checkpoint and expect it to not be
            // already unlocked or otherwise referenced in production.
            //

            // Debug.Log($"Duplicate Detected: {obj.internalId}");
            GenerateInternalId(obj);

            // Register this new item.
            RegisterObject(obj);
        }

        private static void RegisterObject(ScriptableObjectWithID aID, bool replace = false)
        {
            if (replace)
            {
                _stringToObject[aID._internalId] = aID;
            }
            else
            {
                _stringToObject.Add(aID._internalId, aID);
            }

            _objectToString.Add(aID, aID._internalId);
        }

        private static void UnregisterObject(ScriptableObjectWithID aID)
        {
            _stringToObject.Remove(aID._internalId);
            _objectToString.Remove(aID);
        }

        private static void GenerateInternalId(ScriptableObjectWithID obj)
        {
            obj._internalId = Guid.NewGuid().ToString();
            //obj._createdAtTicks = DateTime.Now.Ticks;

            obj._internalIdWasUpdated = true;

            // Debug.Log($"Created Internal ID: {obj.internalId}");
        }
    }
}
