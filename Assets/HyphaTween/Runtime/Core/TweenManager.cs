using AtMycelia.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.HyphaTween
{
    public class TweenManager : MonoBehaviour
    {
        [SerializeField, HideInInspector] private GameObject _tweenAnchorHolder;
        [SerializeField] private DefaultTweenAdapter _defaultAdapter;
        public DefaultTweenAdapter DefaultAdapter => _defaultAdapter;

        public static TweenManager S
        {
            get
            {
                return _s;
            }
            set
            {
                _s = value;
            }
        }
        protected static TweenManager _s;

        protected virtual void Awake()
        {
            if (_s != null && _s != this)
            {
                Debug.LogWarning("Multiple TweenManagers detected. Destroying the new one.");
                Destroy(this.gameObject);
                return;
            }

            ResetAnchors();
            EnsureTweenAnchorHolder();
            _s = this;
        }

        private void ResetAnchors()
        {
            foreach (var kv in _adapterAnchors)
            {
                var anchorFound = kv.Value;
                if (anchorFound == null)
                {
                    continue;
                }

                if (!Application.isPlaying)
                {
                    DestroyImmediate(anchorFound);
                }
                else
                {
                    Destroy(anchorFound);
                }
            }

            _adapterAnchors.Clear();
        }

        private readonly Dictionary<int, GameObject> _adapterAnchors =
            new Dictionary<int, GameObject>();

        // ^What do we use these for? To ensure that when we stop tweens, we don't accidentally
        // stop others that target a different GameObject or component thereof.

        private void EnsureTweenAnchorHolder()
        {
            if (_tweenAnchorHolder == null)
            {
                _tweenAnchorHolder = new GameObject("TweenAnchorHolder");
                _tweenAnchorHolder.transform.SetParent(this.transform, false);
#if UNITY_EDITOR
                _tweenAnchorHolder.hideFlags = HideFlags.HideAndDontSave;
#else
                tweenAnchorHolder.hideFlags = HideFlags.HideInInspector;
#endif
            }
        }

        /// <summary>
        /// Return an existing anchor GameObject for the given adapter (Unity object),
        /// or create one as a child of the manager. Anchor lifetime follows the manager.
        /// </summary>
        public GameObject GetOrCreateAnchorFor(UnityObj unityObj)
        {
            if (unityObj == null) return null;

            EnsureTweenAnchorHolder();

            int key = unityObj.GetInstanceID();

            if (_adapterAnchors.TryGetValue(key, out var existing) && existing != null)
            {
                return existing;
            }

            string anchorName = $"{unityObj.GetType().Name}_AdapterAnchor_{key}";
            Transform found = _tweenAnchorHolder.transform.Find(anchorName);
            if (found != null && found.gameObject != null)
            {
                _adapterAnchors[key] = found.gameObject;
                return found.gameObject;
            }

            GameObject anchor = new GameObject(anchorName);
            anchor.transform.SetParent(_tweenAnchorHolder.transform, false);

#if UNITY_EDITOR
            anchor.hideFlags = HideFlags.HideAndDontSave;
#else
            anchor.hideFlags = HideFlags.HideInInspector;
#endif

            _adapterAnchors[key] = anchor;
            return anchor;
        }

        /// <summary>
        /// Remove and destroy anchor for given adapter (if any).
        /// </summary>
        public void RemoveAnchorFor(UnityObj unityObj)
        {
            if (unityObj == null) return;

            int key = unityObj.GetInstanceID();
            if (_adapterAnchors.TryGetValue(key, out var go) && go != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    Destroy(go);
                }
                else
                {
                    DestroyImmediate(go);
                }
#else
                Destroy(go);
#endif
            }

            _adapterAnchors.Remove(key);
        }

        public void AddTween<T>(Tween<T> toAdd)
        {
            if (_activeTweens.ContainsKey(toAdd.ID))
            {
                _activeTweens[toAdd.ID].OnCompleteKill();
                // ^Since the client may be trying to modify the same property on the same game object
                // as another tween. Thus, we need to do this to avoid issues
            }

            _activeTweens[toAdd.ID] = toAdd;
        }

        protected Dictionary<string, ITween> _activeTweens = new();

        protected virtual void Update()
        {
            _tweensToRemove.Clear();

            foreach (var pair in _activeTweens)
            {
                ITween tween = pair.Value;
                tween.Update();
                if (tween.IsComplete && !tween.WasKilled)
                {
                    tween.OnComplete();
                    tween.OnComplete = delegate { };

                    var tweenAfterOnComplete = _activeTweens[pair.Key];
                    bool replacedTheTween = tweenAfterOnComplete != tween;
                    // ^Like for when OnComplete involves applying a tween of the same type
                    // on the same target as the one the OnComplete belongs to

                    if (!replacedTheTween)
                    {
                        _tweensToRemove.Add(tween);
                    }
                }

                if (tween.WasKilled)
                {
                    _tweensToRemove.Add(tween);
                }
            }

            for (int i = 0; i < _tweensToRemove.Count; i++)
            {
                var tween = _tweensToRemove[i];
                RemoveTween(tween.ID);
            }
        }

        private readonly IList<ITween> _tweensToRemove = new List<ITween>();

        public virtual void RemoveTween(string id)
        {
            _activeTweens.Remove(id);
        }

        /// <summary>
        /// Kills all tween targeting the specified target
        /// </summary>
        public virtual void KillAllOn(object target, bool callOnComplete = true)
        {
            IList<ITween> toCancel = (from elem in _activeTweens.Values
                                      where elem.Target == target
                                      select elem).ToList();
            foreach (var elem in toCancel)
            {
                if (callOnComplete)
                {
                    elem.OnComplete();
                }

                elem.OnCompleteKill();
            }
        }

        public virtual bool IsTweeningOn(object target)
        {
            bool result = (from elem in _activeTweens.Values
                           where elem.Target == target
                           select elem).Count() > 0;

            return result;
        }

        protected virtual void OnDestroy()
        {
            if (_s == this)
            {
                ResetAnchors();
                _s = null;
            }
        }

    }

}